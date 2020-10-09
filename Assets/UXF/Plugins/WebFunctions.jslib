mergeInto(LibraryManager.library, {

    Download: function(text, filename) {
        text = Pointer_stringify(text);
        filename = Pointer_stringify(filename);

        var element = document.createElement('a');
        element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(text));
        element.setAttribute('download', filename);

        element.style.display = 'none';
        document.body.appendChild(element);

        element.click();
    },

    CopyToClipboard: function(text) {
        text = Pointer_stringify(text);
        var elem = document.createElement("textarea");
        document.body.appendChild(elem);
        elem.value = text;
        elem.select();
        document.execCommand("copy");
        document.body.removeChild(elem);
    }

});