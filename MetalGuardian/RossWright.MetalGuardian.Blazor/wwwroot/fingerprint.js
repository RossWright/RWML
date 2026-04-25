window.RossWrightDeviceFingerprint = async function () {
    // Basic browser info
    const basicInfo = {
        userAgent: navigator.userAgent,
        language: navigator.language,
        platform: navigator.platform,
        hardwareConcurrency: navigator.hardwareConcurrency || 'unknown',
        screenResolution: `${window.screen.width}x${window.screen.height}`,
        colorDepth: window.screen.colorDepth,
        timezone: Intl.DateTimeFormat().resolvedOptions().timeZone
    };

    // Canvas fingerprint: Draw text and get data URL
    function getCanvasFingerprint() {
        const canvas = document.createElement('canvas');
        const ctx = canvas.getContext('2d');
        canvas.width = 200;
        canvas.height = 50;
        ctx.textBaseline = 'top';
        ctx.font = '14px Arial';
        ctx.fillStyle = '#f60';
        ctx.fillText('Fingerprint', 10, 10);
        return canvas.toDataURL();
    }

    // WebGL fingerprint: Get vendor/renderer
    function getWebGLFingerprint() {
        const canvas = document.createElement('canvas');
        const gl = canvas.getContext('webgl') || canvas.getContext('experimental-webgl');
        if (!gl) return 'not supported';
        const debugInfo = gl.getExtension('WEBGL_debug_renderer_info');
        if (!debugInfo) return 'extension not available';
        return {
            vendor: gl.getParameter(debugInfo.UNMASKED_VENDOR_WEBGL),
            renderer: gl.getParameter(debugInfo.UNMASKED_RENDERER_WEBGL)
        };
    }

    // Audio fingerprint: Process offline audio context
    async function getAudioFingerprint() {
        const AudioContext = window.OfflineAudioContext || window.webkitOfflineAudioContext;
        if (!AudioContext) return 'not supported';
        const context = new AudioContext(1, 5000, 44100);
        const oscillator = context.createOscillator();
        oscillator.type = 'triangle';
        oscillator.frequency.value = 10000;
        const compressor = context.createDynamicsCompressor();
        compressor.threshold.value = -50;
        compressor.knee.value = 40;
        compressor.ratio.value = 12;
        compressor.attack.value = 0;
        compressor.release.value = 0.25;
        oscillator.connect(compressor);
        compressor.connect(context.destination);
        oscillator.start(0);
        const audioBuffer = await context.startRendering();
        const output = new Float32Array(5000);
        audioBuffer.copyFromChannel(output, 0);
        return output.reduce((acc, val) => acc + Math.abs(val), 0).toString();
    }

    // Fonts probing: Test for common fonts via measurement
    function getFontsFingerprint() {
        const baseFonts = ['monospace', 'sans-serif', 'serif'];
        const testFonts = ['Arial', 'Courier New', 'Georgia', 'Times New Roman', 'Verdana', 'Comic Sans MS']; // Add more for entropy
        const testString = 'abcdefghijklmnopqrstuvwxyz0123456789';
        const canvas = document.createElement('canvas');
        const ctx = canvas.getContext('2d');
        const results = [];
        baseFonts.forEach(base => {
            testFonts.forEach(font => {
                ctx.font = `72px ${font}, ${base}`;
                const width = ctx.measureText(testString).width;
                results.push(`${font}:${width}`);
            });
        });
        return results.join(',');
    }

    // Collect all signals
    const signals = {
        ...basicInfo,
        canvas: getCanvasFingerprint(),
        webgl: JSON.stringify(getWebGLFingerprint()),
        audio: await getAudioFingerprint(),
        fonts: getFontsFingerprint()
    };

    // Hash signals into unique ID (SHA-256)
    const json = JSON.stringify(signals);
    const encoder = new TextEncoder();
    const data = encoder.encode(json);
    const hashBuffer = await crypto.subtle.digest('SHA-256', data);
    const hashArray = Array.from(new Uint8Array(hashBuffer));
    const hashHex = hashArray.map(b => b.toString(16).padStart(2, '0')).join('');
    return hashHex;
};