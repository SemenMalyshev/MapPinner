mergeInto(LibraryManager.library, {
  PickBrowserMedia: function (receiverPtr, directoryPtr, kindPtr) {
    const receiver = UTF8ToString(receiverPtr);
    const directory = UTF8ToString(directoryPtr);
    const kind = UTF8ToString(kindPtr);
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = kind === 'image' ? '.png,.jpg,.jpeg' : '.mp3,.wav,.ogg';
    input.onchange = async function () {
      const file = input.files && input.files[0];
      if (!file) return;
      const extension = file.name.split('.').pop().toLowerCase();
      const allowed = kind === 'image' ? ['png', 'jpg', 'jpeg'] : ['mp3', 'wav', 'ogg'];
      const maxBytes = kind === 'image' ? 15 * 1024 * 1024 : 30 * 1024 * 1024;
      if (!allowed.includes(extension) || file.size > maxBytes) {
        window.alert('Неподдерживаемый формат или слишком большой файл. Лимит: ' + (maxBytes / 1048576) + ' МБ.');
        return;
      }
      try {
        FS.mkdirTree(directory);
        const id = crypto.randomUUID ? crypto.randomUUID() : Date.now() + '-' + Math.random().toString(36).slice(2);
        const path = directory + '/' + id + '.' + extension;
        FS.writeFile(path, new Uint8Array(await file.arrayBuffer()));
        window.mapPinnerStorageReady = (window.mapPinnerStorageReady || Promise.resolve())
          .catch(function () {})
          .then(function () { return new Promise(function (resolve, reject) {
            FS.syncfs(false, function (error) { if (error) reject(error); else resolve(); });
          }); });
        window.mapPinnerStorageReady.then(function () {
          SendMessage(receiver, 'OnBrowserMediaPicked', kind + '|' + path);
        }).catch(function (error) {
            console.error('MapPinner media save failed:', error);
            FS.unlink(path);
            window.alert('Не удалось сохранить файл в браузере.');
        });
      } catch (error) {
        console.error('MapPinner media import failed:', error);
        window.alert('Не удалось импортировать файл. Проверьте свободное место в браузере.');
      }
    };
    input.click();
  },

  PlayBrowserAudio: function (pathPtr) {
    const path = UTF8ToString(pathPtr);
    try {
      if (window.mapPinnerAudio) {
        window.mapPinnerAudio.pause();
        URL.revokeObjectURL(window.mapPinnerAudio.src);
      }
      const extension = path.split('.').pop().toLowerCase();
      const mime = { mp3: 'audio/mpeg', wav: 'audio/wav', ogg: 'audio/ogg' }[extension];
      const url = URL.createObjectURL(new Blob([FS.readFile(path)], { type: mime }));
      const audio = new Audio(url);
      window.mapPinnerAudio = audio;
      audio.onended = function () { URL.revokeObjectURL(url); window.mapPinnerAudio = null; };
      audio.play().catch(function (error) { console.error('MapPinner audio failed:', error); });
    } catch (error) {
      console.error('MapPinner audio failed:', error);
      window.alert('Не удалось воспроизвести аудио.');
    }
  },

  StopBrowserAudio: function () {
    if (!window.mapPinnerAudio) return;
    window.mapPinnerAudio.pause();
    URL.revokeObjectURL(window.mapPinnerAudio.src);
    window.mapPinnerAudio = null;
  },

  SyncBrowserFiles: function () {
    window.mapPinnerStorageReady = (window.mapPinnerStorageReady || Promise.resolve())
      .catch(function () {})
      .then(function () { return new Promise(function (resolve, reject) {
        FS.syncfs(false, function (error) { if (error) reject(error); else resolve(); });
      }); });
    window.mapPinnerStorageReady.catch(function (error) {
      console.error('MapPinner browser save failed:', error);
      window.alert('Не удалось сохранить данные в браузере. Проверьте свободное место.');
    });
  }
});
