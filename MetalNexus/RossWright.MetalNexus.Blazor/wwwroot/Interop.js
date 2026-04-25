window.RossWrightFileInput = {
    attach: function (dotNetObj, inputId) {
        window.RossWrightFileInput.instances[inputId] = dotNetObj;
        var theFileInput = $('#' + inputId);
        theFileInput.change(function () {
            var files = theFileInput.prop('files');
            if (files && files[0]) {
                dotNetObj
                    .invokeMethodAsync('OnFilesSelected',
                        $.map(files, function (file) {
                            var fileRefId = window.RossWrightFileInput.nextFileRefId++;
                            window.RossWrightFileInput.files[fileRefId] = {
                                id: inputId,
                                data: file
                            };
                            return {
                                FileName: file.name,
                                Size: file.size,
                                ContentType: file.type,
                                FileRefId: fileRefId
                            };
                        }))
                    .then(function () {
                        $(inputId).val('');
                    }, function (err) {
                        $(inputId).val('');
                        throw new Error(err);
                    });
            }
            theFileInput.val('');
        });
    },
    click: function (inputId) {
        var dotNetObj = window.RossWrightFileInput.instances[inputId];
        if (!dotNetObj) return;

        var theFileInput = $('#' + inputId);

        var focusHandler = function () {
            window.removeEventListener('focus', focusHandler);
            setTimeout(function () {  // New: Defer check to next event loop tick
                console.log("focusHandler invoke");
                dotNetObj.invokeMethodAsync('OnFilePickerFocusLost');
            }, 100);
        };
        window.addEventListener('focus', focusHandler);

        theFileInput.trigger('click');
    },
    instances: {},
    files: {},
    nextFileRefId: 0,
    showImageUrl: function (imgId, imgUrl, method) {
        if (method == 'imgsrc')
            $('#' + imgId).attr('src', imgUrl);
        else if (method)
            $('#' + imgId).css('background-image', 'url(' + imgUrl + ')');
    },
    showImage: function (imgId, fileRefId, method) {
        var file = window.RossWrightFileInput.files[fileRefId];
        var reader = new FileReader();
        reader.onload = function (e) {
            if (method == 'imgsrc')
                $('#' + imgId).attr('src', e.target.result);
            else if (method)
                $('#' + imgId).css('background-image', 'url(' + e.target.result + ')');
        };
        reader.readAsDataURL(file.data);
    },
    uploadFiles: function (dotNetObj, httpMethod, url, accessToken, fileRefIds) {
        var formData = new FormData();
        for (let fileRefId of fileRefIds) {
            var file = window.RossWrightFileInput.files[fileRefId];
            formData.append('files', file.data);
        }
        var ajaxOptions = {
            url: url,
            method: httpMethod,
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                dotNetObj.invokeMethod('OnSuccess', response);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                if (jqXHR.status == 400)
                    dotNetObj.invokeMethod('OnError', jqXHR.status, jqXHR.responseText);
                else
                    dotNetObj.invokeMethod('OnError', jqXHR.status, null);
            },
            xhr: function () {
                var xhr = new XMLHttpRequest();
                xhr.upload.addEventListener('progress', function (e) {
                    if (e.lengthComputable) {
                        dotNetObj.invokeMethod('OnProgress', e.loaded, e.total);
                    }
                }, false);
                return xhr;
            }
        };
        if (accessToken) {
            ajaxOptions.headers = {
                'Authorization': `Bearer ${accessToken}`
            };
        }
        $.ajax(ajaxOptions);
    },
    detach: function (inputId) {
        window.RossWrightFileInput.files =
            Object.entries(window.RossWrightFileInput.files)
                .filter(function (file) { return file.id != inputId });
        delete window.RossWrightFileInput.instances[inputId];
        $('#' + inputId).change();
    }
};