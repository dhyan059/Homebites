// Shared browser helpers used by customer and admin pages.
(function (window) {
  var apiBase = 'http://localhost:5000/api';

  window.HomebitesRuntime = {
    apiBase: apiBase,

    async request(path, options) {
      var response = await fetch(apiBase + path, options);
      var data = await response.json().catch(function () { return {}; });
      if (!response.ok) {
        var error = new Error(data.message || 'The request failed.');
        error.status = response.status;
        error.data = data;
        throw error;
      }
      return data;
    },

    currentUser() {
      try {
        return JSON.parse(localStorage.getItem('homebites_user')) || null;
      } catch (error) {
        return null;
      }
    },

    requireUser() {
      var user = this.currentUser();
      if (!user) {
        window.location.href = 'login.html';
        return null;
      }
      return user;
    },

    escapeHtml(value) {
      return String(value == null ? '' : value).replace(/[&<>\"']/g, function (character) {
        return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '\"': '&quot;', "'": '&#39;' }[character];
      });
    }
  };
})(window);
