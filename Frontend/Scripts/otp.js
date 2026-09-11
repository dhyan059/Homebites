// OTP page helpers.
(function (window) {
	window.HomebitesOtp = {
		isValid(code) {
			return /^\d{6}$/.test(String(code || '').trim());
		}
	};
})(window);
