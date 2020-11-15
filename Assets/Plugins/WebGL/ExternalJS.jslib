// from: https://forum.unity.com/threads/how-does-saving-work-in-webgl.390385/

mergeInto(LibraryManager.library, {
    //put into Assets/Plugins/WebGL/HandleIO.jslib
    SyncFiles: function () {
        FS.syncfs(false, function (err) {
            // handle callback
        });
    },
});