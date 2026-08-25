// Navigation Component JavaScript
// Mobile menu toggle functionality

(function() {
  'use strict';

  // Get DOM elements
  const navbarToggle = document.querySelector('.navbar-toggle');
  const navbarMenu = document.querySelector('.navbar-menu');

  if (!navbarToggle || !navbarMenu) {
    return;
  }

  // Toggle mobile menu
  function toggleMenu() {
    const isExpanded = navbarToggle.getAttribute('aria-expanded') === 'true';

    navbarToggle.setAttribute('aria-expanded', !isExpanded);
    navbarMenu.classList.toggle('active');
  }

  // Close menu when clicking outside
  function closeMenuOnOutsideClick(event) {
    if (!navbarMenu.contains(event.target) && !navbarToggle.contains(event.target)) {
      if (navbarMenu.classList.contains('active')) {
        toggleMenu();
      }
    }
  }

  // Close menu on escape key
  function closeMenuOnEscape(event) {
    if (event.key === 'Escape' && navbarMenu.classList.contains('active')) {
      toggleMenu();
      navbarToggle.focus();
    }
  }

  // Event listeners
  navbarToggle.addEventListener('click', toggleMenu);
  document.addEventListener('click', closeMenuOnOutsideClick);
  document.addEventListener('keydown', closeMenuOnEscape);

  // Close menu when window is resized above mobile breakpoint
  let resizeTimer;
  window.addEventListener('resize', function() {
    clearTimeout(resizeTimer);
    resizeTimer = setTimeout(function() {
      if (window.innerWidth > 768 && navbarMenu.classList.contains('active')) {
        navbarMenu.classList.remove('active');
        navbarToggle.setAttribute('aria-expanded', 'false');
      }
    }, 250);
  });

  // Set active nav item based on current page
  function setActiveNavItem() {
    const currentPath = window.location.pathname;
    const navItems = document.querySelectorAll('.nav-item');

    navItems.forEach(function(item) {
      const itemPath = new URL(item.href).pathname;

      if (currentPath === itemPath || (currentPath === '/' && itemPath.endsWith('index.html'))) {
        item.classList.add('nav-item-active');
      } else {
        item.classList.remove('nav-item-active');
      }
    });
  }

  // Initialize
  setActiveNavItem();
})();
