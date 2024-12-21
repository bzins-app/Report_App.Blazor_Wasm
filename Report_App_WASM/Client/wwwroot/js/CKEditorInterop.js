window.CKEditorInterop = (() => {
    const editors = {};

    return {
        init(id, dotNetReference) {
            ClassicEditor
                .create(document.getElementById(id))
                .then(editor => {
                    editors[id] = editor;
                    editor.model.document.on('change:data', () => {
                        const data = editor.getData().trim();
                        const el = document.createElement('div');
                        el.innerHTML = data;
                        const cleanedData = el.innerText.trim() === '' ? null : data;

                        dotNetReference.invokeMethodAsync('EditorDataChanged', cleanedData);
                    });
                })
                .catch(error => console.error(error));
        },
        destroy(id) {
            if (editors[id]) {
                editors[id].destroy()
                    .then(() => delete editors[id])
                    .catch(error => console.error(error));
            }
        }
    };
})();