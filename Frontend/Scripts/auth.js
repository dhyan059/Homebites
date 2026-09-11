// Homebites Central API, State, Authentication, and Strict Validations
const API_BASE = 'http://localhost:5000/api';

const Homebites = {
  // Current user in localStorage (Isolated per user session)
  getUser() {
    try {
      return JSON.parse(localStorage.getItem('homebites_user')) || null;
    } catch (e) {
      return null;
    }
  },

  setUser(user) {
    localStorage.setItem('homebites_user', JSON.stringify(user));
    Homebites.renderAuthNav();
  },

  logout() {
    localStorage.removeItem('homebites_user');
    localStorage.removeItem('homebites_cart');
    localStorage.removeItem('homebites_selected_address_id');
    Homebites.showToast('You have been logged out safely.');
    setTimeout(() => {
      window.location.href = window.location.pathname.includes('/Customer/') ? '../index.html' : 'index.html';
    }, 800);
  },

  renderAuthNav() {
    const user = Homebites.getUser();
    const navActions = document.querySelector('.nav-actions');
    if (!navActions) return;

    if (user) {
      const isCustomerDir = window.location.pathname.includes('/Customer/');
      const basePrefix = isCustomerDir ? '' : 'Customer/';
      
      navActions.innerHTML = `
        <div style="display:flex;align-items:center;gap:12px;">
          <a href="${basePrefix}profile.html" style="display:flex;align-items:center;gap:8px;padding:8px 16px;background:rgba(255,255,255,0.08);border:1px solid rgba(255,107,53,0.3);border-radius:50px;font-size:13.5px;font-weight:600;color:#fff;">
            <i class="fa-solid fa-user" style="color:var(--orange);"></i> ${user.fullName.split(' ')[0]}
          </a>
          <a href="${basePrefix}cart.html" style="position:relative;width:40px;height:40px;border-radius:12px;background:rgba(255,107,53,0.15);border:1px solid rgba(255,107,53,0.35);color:var(--orange);display:flex;align-items:center;justify-content:center;font-size:16px;">
            <i class="fa-solid fa-cart-shopping"></i>
            <span class="cart-count-badge" style="position:absolute;top:-6px;right:-6px;background:var(--orange);color:#fff;font-size:10px;font-weight:800;border-radius:50%;padding:2px 6px;display:none;">0</span>
          </a>
          <button onclick="Homebites.logout()" style="padding:9px 18px;border-radius:50px;border:1.5px solid rgba(230,57,70,0.5);color:#ff4d6d;font-weight:600;font-size:13px;cursor:pointer;background:rgba(230,57,70,0.1);display:flex;align-items:center;gap:6px;">
            <i class="fa-solid fa-arrow-right-from-bracket"></i> Logout
          </button>
          <button class="side-menu-trigger" onclick="openDrawer()" title="Menu Access" style="width:40px;height:40px;border-radius:12px;background:rgba(255,107,53,0.15);border:1.5px solid rgba(255,107,53,0.35);color:var(--orange);font-size:17px;cursor:pointer;display:flex;align-items:center;justify-content:center;">
            <i class="fa-solid fa-plus"></i>
          </button>
        </div>
      `;
    } else {
      navActions.innerHTML = `
        <button class="btn-outline" onclick="openModal('login')">Login</button>
        <button class="btn-primary" onclick="openModal('register')">Sign Up</button>
        <button class="side-menu-trigger" onclick="openDrawer()" title="More Menu & Account Access">
          <i class="fa-solid fa-plus"></i>
        </button>
      `;
    }
    Homebites.updateCartBadge();
  },

  // Cart in localStorage (tied to individual user session)
  getCart() {
    try {
      const user = Homebites.getUser();
      const key = user ? `homebites_cart_${user.id}` : 'homebites_cart_guest';
      return JSON.parse(localStorage.getItem(key)) || [];
    } catch (e) {
      return [];
    }
  },

  saveCart(cart) {
    const user = Homebites.getUser();
    const key = user ? `homebites_cart_${user.id}` : 'homebites_cart_guest';
    localStorage.setItem(key, JSON.stringify(cart));
    Homebites.updateCartBadge();
  },

  addToCart(meal) {
    const cart = Homebites.getCart();
    const existing = cart.find(item => item.mealId === meal.mealId);
    if (existing) {
      existing.quantity += 1;
    } else {
      cart.push({
        mealId: meal.mealId,
        mealName: meal.mealName,
        price: meal.price,
        discountPrice: meal.discountPrice || meal.price,
        isVeg: meal.isVeg,
        imageUrl: meal.imageUrl || 'https://images.unsplash.com/photo-1546833999-b9f581a1996d?auto=format&fit=crop&w=600&q=80',
        quantity: 1
      });
    }
    Homebites.saveCart(cart);
    Homebites.showToast(`Added "${meal.mealName}" to cart! 🛒`);
  },

  updateCartBadge() {
    const cart = Homebites.getCart();
    const totalCount = cart.reduce((sum, item) => sum + item.quantity, 0);
    const badges = document.querySelectorAll('.cart-count-badge');
    badges.forEach(b => {
      b.textContent = totalCount;
      b.style.display = totalCount > 0 ? 'inline-block' : 'none';
    });
  },

  // Strict Validations
  validateGmail(email) {
    if (!email) return { valid: false, message: 'Email address is required.' };
    const clean = email.trim().toLowerCase();
    const regex = /^[a-zA-Z0-9._%+-]+@gmail\.com$/;
    if (!regex.test(clean) || !clean.endsWith('@gmail.com')) {
      return { valid: false, message: 'Email must end strictly with @gmail.com (e.g. yourname@gmail.com).' };
    }
    return { valid: true, email: clean };
  },

  validateMobile(mobile) {
    if (!mobile) return { valid: false, message: 'Mobile number is required.' };
    let clean = mobile.trim().replace(/\s+/g, '');
    if (/^[6-9]\d{9}$/.test(clean)) {
      clean = '+91' + clean;
    }
    const regex = /^\+91[6-9]\d{9}$/;
    if (!regex.test(clean) || clean.length !== 13) {
      return { valid: false, message: 'Mobile number must start with +91 followed by a valid 10-digit number (e.g. +919876543210).' };
    }
    return { valid: true, mobile: clean };
  },

  validatePincode(pincode) {
    if (!pincode) return { valid: false, message: 'Pincode is required.' };
    const clean = pincode.trim();
    if (!/^[1-9]\d{5}$/.test(clean) || clean.length !== 6) {
      return { valid: false, message: 'Pincode must be exactly 6 numeric digits (e.g. 500081).' };
    }
    return { valid: true, pincode: clean };
  },

  showToast(message, isError = false) {
    let container = document.getElementById('hb-toast-container');
    if (!container) {
      container = document.createElement('div');
      container.id = 'hb-toast-container';
      container.style.cssText = 'position:fixed;bottom:24px;right:24px;z-index:99999;display:flex;flex-direction:column;gap:10px;pointer-events:none;';
      document.body.appendChild(container);
    }

    const toast = document.createElement('div');
    toast.style.cssText = `
      background: ${isError ? 'rgba(230,57,70,0.96)' : 'rgba(22,33,62,0.96)'};
      border: 1px solid ${isError ? '#ff4d6d' : '#FF6B35'};
      color: #FFFFFF;
      padding: 14px 20px;
      border-radius: 12px;
      box-shadow: 0 10px 30px rgba(0,0,0,0.45);
      font-family: 'Poppins', sans-serif;
      font-size: 14px;
      font-weight: 600;
      backdrop-filter: blur(10px);
      transition: all 0.3s cubic-bezier(0.25, 0.8, 0.25, 1);
      transform: translateY(20px);
      opacity: 0;
      pointer-events: auto;
    `;
    toast.innerHTML = message;
    container.appendChild(toast);

    setTimeout(() => {
      toast.style.transform = 'translateY(0)';
      toast.style.opacity = '1';
    }, 20);

    setTimeout(() => {
      toast.style.transform = 'translateY(-10px)';
      toast.style.opacity = '0';
      setTimeout(() => toast.remove(), 300);
    }, 3800);
  }
};

document.addEventListener('DOMContentLoaded', () => {
  Homebites.renderAuthNav();
});
