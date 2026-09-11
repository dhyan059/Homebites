// Checkout page helpers.
(function (window) {
	window.HomebitesCheckout = {
		cartTotal(cart) {
			return window.HomebitesCart ? window.HomebitesCart.totals(cart).total : 0;
		}
	};
})(window);
