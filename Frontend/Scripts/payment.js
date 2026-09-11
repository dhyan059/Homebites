// Payment page helpers.
(function (window) {
	window.HomebitesPayment = {
		isSupportedMethod(method) {
			return ['UPI', 'Card', 'CashOnDelivery'].indexOf(method) !== -1;
		}
	};
})(window);
