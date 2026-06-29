/**
 * Vitest unit tests for Interop.js
 *
 * The module is loaded by injecting its source into a jsdom window that has a
 * minimal jQuery shim and browser-API stubs.  Each test rebuilds the state
 * from scratch so tests are fully independent.
 */

import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { readFileSync } from 'fs';
import { fileURLToPath } from 'url';
import { dirname, join } from 'path';

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

const __dirname = dirname(fileURLToPath(import.meta.url));
const interopSource = readFileSync(join(__dirname, 'Interop.js'), 'utf-8');

/** Reset global state and re-evaluate Interop.js before every test. */
function loadInterop() {
    // Remove any previous instance so each test starts clean
    delete window.RossWrightFileInput;

    // Minimal jQuery shim that covers the subset used by Interop.js:
    //   $(selector)  ->  object with .change(), .prop(), .val(), .trigger(), .attr(), .css()
    const elements = {};
    function getEl(selector) {
        const key = String(selector);
        if (!elements[key]) {
            elements[key] = {
                _changeHandlers: [],
                _val: '',
                change(fn) {
                    if (fn) this._changeHandlers.push(fn);
                    else this._changeHandlers.forEach(h => h());
                    return this;
                },
                prop(name) {
                    if (name === 'files') return this._files || null;
                    return undefined;
                },
                val(v) {
                    if (v !== undefined) { this._val = v; return this; }
                    return this._val;
                },
                trigger(ev) {
                    if (ev === 'click') this._clicked = true;
                    return this;
                },
                attr(name, val) {
                    if (val !== undefined) { this._attrs = this._attrs || {}; this._attrs[name] = val; }
                    return this;
                },
                css(prop, val) {
                    if (val !== undefined) { this._css = this._css || {}; this._css[prop] = val; }
                    return this;
                },
            };
        }
        return elements[key];
    }

    window.$ = function (selector) { return getEl(selector); };
    window.$.map = (arr, fn) => arr.map(fn);
    window._jqElements = elements;

    // Evaluate the source in the current jsdom window context
    // eslint-disable-next-line no-new-func
    new Function(interopSource)();

    return window.RossWrightFileInput;
}

// ---------------------------------------------------------------------------
// attach
// ---------------------------------------------------------------------------

describe('attach', () => {
    let rw;

    beforeEach(() => { rw = loadInterop(); });

    it('registers the dotNetObj in instances', () => {
        const dotNetObj = { invokeMethodAsync: vi.fn().mockResolvedValue(undefined) };
        rw.attach(dotNetObj, 'input1');
        expect(rw.instances['input1']).toBe(dotNetObj);
    });

    it('invokes OnFilesSelected with correct BrowserFile shape when files are selected', async () => {
        const dotNetObj = {
            invokeMethodAsync: vi.fn().mockResolvedValue(undefined),
        };
        rw.attach(dotNetObj, 'input2');

        const mockFile = { name: 'photo.jpg', size: 12345, type: 'image/jpeg' };
        const el = window._jqElements['#input2'];
        el._files = [mockFile];

        // Simulate the change event
        el._changeHandlers[0]();
        // The handler is async via .then; flush microtasks
        await new Promise(r => setTimeout(r, 0));

        expect(dotNetObj.invokeMethodAsync).toHaveBeenCalledWith('OnFilesSelected', [
            expect.objectContaining({
                FileName: 'photo.jpg',
                Size: 12345,
                ContentType: 'image/jpeg',
                FileRefId: expect.any(Number),
            }),
        ]);
    });

    it('stores file data in files map with the correct inputId', async () => {
        const dotNetObj = { invokeMethodAsync: vi.fn().mockResolvedValue(undefined) };
        rw.attach(dotNetObj, 'input3');

        const mockFile = { name: 'doc.pdf', size: 500, type: 'application/pdf' };
        const el = window._jqElements['#input3'];
        el._files = [mockFile];

        el._changeHandlers[0]();
        await new Promise(r => setTimeout(r, 0));

        const storedEntry = Object.values(rw.files).find(f => f.data === mockFile);
        expect(storedEntry).toBeDefined();
        expect(storedEntry.id).toBe('input3');
    });

    it('assigns incrementing FileRefIds across multiple attach calls', async () => {
        rw.nextFileRefId = 0;

        for (const id of ['i1', 'i2']) {
            const dn = { invokeMethodAsync: vi.fn().mockResolvedValue(undefined) };
            rw.attach(dn, id);
            const el = window._jqElements[`#${id}`];
            el._files = [{ name: 'x.txt', size: 1, type: 'text/plain' }];
            el._changeHandlers[el._changeHandlers.length - 1]();
            await new Promise(r => setTimeout(r, 0));
        }

        const ids = Object.keys(rw.files).map(Number).sort((a, b) => a - b);
        expect(ids[1]).toBe(ids[0] + 1);
    });

    it('clears the input value after files are selected', async () => {
        const dotNetObj = { invokeMethodAsync: vi.fn().mockResolvedValue(undefined) };
        rw.attach(dotNetObj, 'input4');

        const el = window._jqElements['#input4'];
        el._files = [{ name: 'a.txt', size: 1, type: 'text/plain' }];
        el._changeHandlers[0]();
        await new Promise(r => setTimeout(r, 0));

        expect(el._val).toBe('');
    });

    it('does not invoke OnFilesSelected when files is empty/null', async () => {
        const dotNetObj = { invokeMethodAsync: vi.fn().mockResolvedValue(undefined) };
        rw.attach(dotNetObj, 'input5');

        const el = window._jqElements['#input5'];
        el._files = null;
        el._changeHandlers[0]();
        await new Promise(r => setTimeout(r, 0));

        expect(dotNetObj.invokeMethodAsync).not.toHaveBeenCalled();
    });

    it('clears input even when files is null/empty', async () => {
        const dotNetObj = { invokeMethodAsync: vi.fn().mockResolvedValue(undefined) };
        rw.attach(dotNetObj, 'input6');

        const el = window._jqElements['#input6'];
        el._files = null;
        el._changeHandlers[0]();
        await new Promise(r => setTimeout(r, 0));

        expect(el._val).toBe('');
    });
});

// ---------------------------------------------------------------------------
// click
// ---------------------------------------------------------------------------

describe('click', () => {
    let rw;

    beforeEach(() => { rw = loadInterop(); });

    it('does nothing when instance is not registered', () => {
        expect(() => rw.click('unknown')).not.toThrow();
    });

    it('triggers click on the file input element', () => {
        const dotNetObj = { invokeMethodAsync: vi.fn().mockResolvedValue(undefined) };
        rw.instances['inp'] = dotNetObj;

        rw.click('inp');

        expect(window._jqElements['#inp']._clicked).toBe(true);
    });

    it('adds a focus event listener on window', () => {
        const addSpy = vi.spyOn(window, 'addEventListener');
        const dotNetObj = { invokeMethodAsync: vi.fn().mockResolvedValue(undefined) };
        rw.instances['inp2'] = dotNetObj;

        rw.click('inp2');

        expect(addSpy).toHaveBeenCalledWith('focus', expect.any(Function));
        addSpy.mockRestore();
    });

    it('calls OnFilePickerFocusLost after focus event fires', async () => {
        vi.useFakeTimers();

        const dotNetObj = { invokeMethodAsync: vi.fn().mockResolvedValue(undefined) };
        rw.instances['inp3'] = dotNetObj;

        let capturedHandler;
        const origAdd = window.addEventListener.bind(window);
        vi.spyOn(window, 'addEventListener').mockImplementation((ev, fn) => {
            if (ev === 'focus') capturedHandler = fn;
            else origAdd(ev, fn);
        });

        rw.click('inp3');

        // Simulate window regaining focus (file picker dismissed)
        capturedHandler();
        await vi.runAllTimersAsync();

        expect(dotNetObj.invokeMethodAsync).toHaveBeenCalledWith('OnFilePickerFocusLost');

        vi.useRealTimers();
        vi.restoreAllMocks();
    });

    it('removes focus listener after it fires once', async () => {
        vi.useFakeTimers();

        const dotNetObj = { invokeMethodAsync: vi.fn().mockResolvedValue(undefined) };
        rw.instances['inp4'] = dotNetObj;

        const removeSpy = vi.spyOn(window, 'removeEventListener');

        let capturedHandler;
        vi.spyOn(window, 'addEventListener').mockImplementation((ev, fn) => {
            if (ev === 'focus') capturedHandler = fn;
        });

        rw.click('inp4');
        capturedHandler();
        await vi.runAllTimersAsync();

        expect(removeSpy).toHaveBeenCalledWith('focus', capturedHandler);

        vi.useRealTimers();
        vi.restoreAllMocks();
    });
});

// ---------------------------------------------------------------------------
// showImageUrl
// ---------------------------------------------------------------------------

describe('showImageUrl', () => {
    let rw;

    beforeEach(() => { rw = loadInterop(); });

    it('sets src attribute when method is imgsrc', () => {
        rw.showImageUrl('myImg', 'https://example.com/photo.jpg', 'imgsrc');
        expect(window._jqElements['#myImg']._attrs?.src).toBe('https://example.com/photo.jpg');
    });

    it('sets background-image CSS when method is a non-imgsrc truthy string', () => {
        rw.showImageUrl('myDiv', 'https://example.com/bg.jpg', 'cssbgimg');
        expect(window._jqElements['#myDiv']._css?.['background-image']).toBe('url(https://example.com/bg.jpg)');
    });

    it('does not modify element when method is falsy', () => {
        rw.showImageUrl('el', 'https://example.com/x.jpg', '');
        // The element may not even exist in the shim; either way no src or background was set
        const el = window._jqElements['#el'];
        expect(el?._attrs?.src).toBeUndefined();
        expect(el?._css?.['background-image']).toBeUndefined();
    });
});

// ---------------------------------------------------------------------------
// showImage
// ---------------------------------------------------------------------------

describe('showImage', () => {
    let rw;

    beforeEach(() => { rw = loadInterop(); });

    it('reads file via FileReader and sets src when method is imgsrc', async () => {
        const mockData = {};
        rw.files[42] = { id: 'input', data: mockData };

        // Stub FileReader
        const readResult = 'data:image/jpeg;base64,abc123';
        const mockReader = {
            onload: null,
            readAsDataURL(file) {
                // Synchronously invoke the onload callback
                this.onload({ target: { result: readResult } });
            },
        };
        window.FileReader = vi.fn(() => mockReader);

        rw.showImage('previewImg', 42, 'imgsrc');
        await new Promise(r => setTimeout(r, 0));

        expect(window._jqElements['#previewImg']._attrs?.src).toBe(readResult);
    });

    it('sets background-image CSS when method is non-imgsrc truthy string', async () => {
        const mockData = {};
        rw.files[43] = { id: 'input', data: mockData };

        const readResult = 'data:image/png;base64,xyz';
        const mockReader = {
            onload: null,
            readAsDataURL() { this.onload({ target: { result: readResult } }); },
        };
        window.FileReader = vi.fn(() => mockReader);

        rw.showImage('bgDiv', 43, 'cssbgimg');
        await new Promise(r => setTimeout(r, 0));

        expect(window._jqElements['#bgDiv']._css?.['background-image']).toBe(`url(${readResult})`);
    });
});

// ---------------------------------------------------------------------------
// uploadFiles
// ---------------------------------------------------------------------------

describe('uploadFiles', () => {
    let rw;
    let mockXhr;
    let ajaxOptions;

    beforeEach(() => {
        rw = loadInterop();

        // Stub $.ajax to capture options without actually sending a request
        window.$.ajax = vi.fn(opts => { ajaxOptions = opts; });

        // Seed two files in the registry
        rw.files[1] = { id: 'input', data: { name: 'a.jpg', size: 100, type: 'image/jpeg' } };
        rw.files[2] = { id: 'input', data: { name: 'b.pdf', size: 200, type: 'application/pdf' } };

        // Stub FormData
        const entries = [];
        window.FormData = vi.fn(() => ({
            _entries: entries,
            append(name, val) { entries.push({ name, val }); },
            getEntries() { return entries; },
        }));
    });

    it('appends each file entry using the provided fieldName', () => {
        const dotNetObj = { invokeMethod: vi.fn() };
        const fileEntries = [
            { fileRefId: 1, fieldName: 'avatar' },
            { fileRefId: 2, fieldName: 'resume' },
        ];

        rw.uploadFiles(dotNetObj, 'POST', '/api/upload', null, fileEntries);

        const fd = window.FormData.mock.results[0].value;
        expect(fd._entries).toEqual([
            { name: 'avatar', val: rw.files[1].data },
            { name: 'resume', val: rw.files[2].data },
        ]);
    });

    it('uses the provided HTTP method and URL', () => {
        const dotNetObj = { invokeMethod: vi.fn() };
        rw.uploadFiles(dotNetObj, 'PUT', '/api/items/5', null, [{ fileRefId: 1, fieldName: 'files' }]);

        expect(ajaxOptions.url).toBe('/api/items/5');
        expect(ajaxOptions.method).toBe('PUT');
    });

    it('adds Authorization header when accessToken is provided', () => {
        const dotNetObj = { invokeMethod: vi.fn() };
        rw.uploadFiles(dotNetObj, 'POST', '/api/upload', 'my-token', [{ fileRefId: 1, fieldName: 'files' }]);

        expect(ajaxOptions.headers).toEqual({ Authorization: 'Bearer my-token' });
    });

    it('omits Authorization header when accessToken is null/falsy', () => {
        const dotNetObj = { invokeMethod: vi.fn() };
        rw.uploadFiles(dotNetObj, 'POST', '/api/upload', null, [{ fileRefId: 1, fieldName: 'files' }]);

        expect(ajaxOptions.headers).toBeUndefined();
    });

    it('calls OnSuccess with response on success', () => {
        const dotNetObj = { invokeMethod: vi.fn() };
        rw.uploadFiles(dotNetObj, 'POST', '/api/upload', null, [{ fileRefId: 1, fieldName: 'files' }]);

        ajaxOptions.success('{"id":1}');

        expect(dotNetObj.invokeMethod).toHaveBeenCalledWith('OnSuccess', '{"id":1}');
    });

    it('calls OnError with status and responseText on 400', () => {
        const dotNetObj = { invokeMethod: vi.fn() };
        rw.uploadFiles(dotNetObj, 'POST', '/api/upload', null, [{ fileRefId: 1, fieldName: 'files' }]);

        ajaxOptions.error({ status: 400, responseText: 'Bad request detail' }, 'error', 'Bad Request');

        expect(dotNetObj.invokeMethod).toHaveBeenCalledWith('OnError', 400, 'Bad request detail');
    });

    it('calls OnError with null responseText for non-400 errors', () => {
        const dotNetObj = { invokeMethod: vi.fn() };
        rw.uploadFiles(dotNetObj, 'POST', '/api/upload', null, [{ fileRefId: 1, fieldName: 'files' }]);

        ajaxOptions.error({ status: 500, responseText: 'Internal' }, 'error', 'Server Error');

        expect(dotNetObj.invokeMethod).toHaveBeenCalledWith('OnError', 500, null);
    });

    it('partitions aggregate progress bytes across files in submission order', () => {
        const dotNetObj = { invokeMethod: vi.fn() };
        // file 1: 100 bytes, file 2: 200 bytes
        rw.uploadFiles(dotNetObj, 'POST', '/api/upload', null, [
            { fileRefId: 1, fieldName: 'files' },
            { fileRefId: 2, fieldName: 'files' },
        ]);

        // Get the xhr factory from ajaxOptions and simulate a progress event
        const xhrObj = ajaxOptions.xhr();

        // Trigger a progress event: 150 out of 300 total bytes loaded
        const listeners = [];
        xhrObj.upload.addEventListener = vi.fn((ev, fn) => { listeners.push(fn); });
        // Rebuild by re-running uploadFiles so the listener is registered on the mock
        window.$.ajax = vi.fn(opts => {
            ajaxOptions = opts;
            const xhr = opts.xhr();
            xhr.upload._listeners = xhr.upload._listeners || [];
        });

        // Build a minimal mock that captures the progress listener
        let progressListener;
        const xhrMock = {
            upload: {
                addEventListener(ev, fn) { if (ev === 'progress') progressListener = fn; },
            },
        };
        window.XMLHttpRequest = vi.fn(() => xhrMock);

        rw.uploadFiles(dotNetObj, 'POST', '/api/upload', null, [
            { fileRefId: 1, fieldName: 'files' },
            { fileRefId: 2, fieldName: 'files' },
        ]);

        // Fire a progress event: 150 of 300 bytes sent
        progressListener({ lengthComputable: true, loaded: 150, total: 300 });

        const call = dotNetObj.invokeMethod.mock.calls.find(c => c[0] === 'OnProgress');
        expect(call).toBeDefined();
        const [, loaded, total, perFile] = call;
        expect(loaded).toBe(150);
        expect(total).toBe(300);

        // First file (100 bytes): all 100 loaded (since 150 > 100)
        expect(perFile[0].FileName).toBe('a.jpg');
        expect(perFile[0].Loaded).toBe(100);
        expect(perFile[0].Total).toBe(100);

        // Second file (200 bytes): remaining 50 bytes loaded
        expect(perFile[1].FileName).toBe('b.pdf');
        expect(perFile[1].Loaded).toBe(50);
        expect(perFile[1].Total).toBe(200);
    });

    it('does not call OnProgress when lengthComputable is false', () => {
        const dotNetObj = { invokeMethod: vi.fn() };
        let progressListener;
        const xhrMock = {
            upload: {
                addEventListener(ev, fn) { if (ev === 'progress') progressListener = fn; },
            },
        };
        window.XMLHttpRequest = vi.fn(() => xhrMock);

        rw.uploadFiles(dotNetObj, 'POST', '/api/upload', null, [{ fileRefId: 1, fieldName: 'files' }]);
        // Invoke the xhr factory so the progress listener is registered
        ajaxOptions.xhr();
        progressListener({ lengthComputable: false, loaded: 0, total: 0 });

        expect(dotNetObj.invokeMethod).not.toHaveBeenCalledWith('OnProgress', expect.anything(), expect.anything(), expect.anything());
    });

    it('disables processData and contentType on the ajax request', () => {
        const dotNetObj = { invokeMethod: vi.fn() };
        rw.uploadFiles(dotNetObj, 'POST', '/api/upload', null, [{ fileRefId: 1, fieldName: 'files' }]);

        expect(ajaxOptions.processData).toBe(false);
        expect(ajaxOptions.contentType).toBe(false);
    });
});

// ---------------------------------------------------------------------------
// detach
// ---------------------------------------------------------------------------

describe('detach', () => {
    let rw;

    beforeEach(() => { rw = loadInterop(); });

    it('removes the dotNetObj from instances', () => {
        rw.instances['i1'] = { invokeMethodAsync: vi.fn() };
        rw.detach('i1');
        expect(rw.instances['i1']).toBeUndefined();
    });

    it('removes files belonging to the detached input', () => {
        rw.files[10] = { id: 'i1', data: {} };
        rw.files[11] = { id: 'i2', data: {} };
        rw.instances['i1'] = {};
        rw.detach('i1');

        // File for i2 must remain; file for i1 must be gone
        expect(rw.files[11]).toBeDefined();
        expect(rw.files[10]).toBeUndefined();
    });

    it('does not throw when detaching an unregistered id', () => {
        expect(() => rw.detach('nonexistent')).not.toThrow();
    });
});
