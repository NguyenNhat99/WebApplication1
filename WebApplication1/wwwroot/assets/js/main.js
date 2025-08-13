/*!
 * Yummy main.js - safe version
 * - Null guards cho element có thể không tồn tại
 * - Kiểm tra thư viện ngoài trước khi init (AOS, GLightbox, Swiper, PureCounter)
 * - Giữ nguyên hành vi: header scrolled, mobile nav, dropdown, preloader, scroll-top, AOS, lightbox, swiper, scrollspy
 */

(function () {
    "use strict";

    /* ========== Header scrolled ========== */
    function toggleScrolled() {
        const body = document.body;
        const header = document.querySelector('#header');
        if (!header) return;

        const stickyEnabled =
            header.classList.contains('scroll-up-sticky') ||
            header.classList.contains('sticky-top') ||
            header.classList.contains('fixed-top');

        if (!stickyEnabled) return;

        if (window.scrollY > 100) body.classList.add('scrolled');
        else body.classList.remove('scrolled');
    }
    document.addEventListener('scroll', toggleScrolled);
    window.addEventListener('load', toggleScrolled);

    /* ========== Mobile nav toggle ========== */
    const mobileNavToggleBtn = document.querySelector('.mobile-nav-toggle');
    function mobileNavToggle() {
        document.body.classList.toggle('mobile-nav-active');
        if (mobileNavToggleBtn) {
            mobileNavToggleBtn.classList.toggle('bi-list');
            mobileNavToggleBtn.classList.toggle('bi-x');
        }
    }
    if (mobileNavToggleBtn) {
        mobileNavToggleBtn.addEventListener('click', mobileNavToggle);
    }

    /* ========== Hide mobile nav on same-page/hash links ========== */
    document.querySelectorAll('#navmenu a').forEach((link) => {
        link.addEventListener('click', () => {
            if (document.body.classList.contains('mobile-nav-active')) {
                mobileNavToggle();
            }
        });
    });

    /* ========== Toggle mobile nav dropdowns ========== */
    document.querySelectorAll('.navmenu .toggle-dropdown').forEach((toggler) => {
        toggler.addEventListener('click', function (e) {
            e.preventDefault();
            const parent = this.parentNode; // <a> parent <li>
            if (parent) parent.classList.toggle('active');
            const next = parent ? parent.nextElementSibling : null; // <ul> dropdown
            if (next) next.classList.toggle('dropdown-active');
            e.stopImmediatePropagation();
        });
    });

    /* ========== Preloader ========== */
    const preloader = document.querySelector('#preloader');
    if (preloader) {
        window.addEventListener('load', () => preloader.remove());
    }

    /* ========== Scroll top button ========== */
    const scrollTop = document.querySelector('.scroll-top');
    function toggleScrollTop() {
        if (!scrollTop) return;
        if (window.scrollY > 100) scrollTop.classList.add('active');
        else scrollTop.classList.remove('active');
    }
    if (scrollTop) {
        scrollTop.addEventListener('click', (e) => {
            e.preventDefault();
            window.scrollTo({ top: 0, behavior: 'smooth' });
        });
    }
    window.addEventListener('load', toggleScrollTop);
    document.addEventListener('scroll', toggleScrollTop);

    /* ========== AOS ========== */
    function aosInit() {
        if (window.AOS && typeof AOS.init === 'function') {
            AOS.init({
                duration: 600,
                easing: 'ease-in-out',
                once: true,
                mirror: false
            });
            // refresh để bắt kịp layout động (ảnh, swiper…)
            setTimeout(() => { if (window.AOS) AOS.refresh(); }, 300);
        }
    }
    window.addEventListener('load', aosInit);

    /* ========== GLightbox ========== */
    if (window.GLightbox && typeof GLightbox === 'function') {
        GLightbox({
            selector: '.glightbox',
            closeButton: true,
            touchNavigation: true,
            loop: false
        });
    }

    /* ========== PureCounter ========== */
    if (window.PureCounter) {
        new PureCounter();
    }

    /* ========== Swiper sliders ========== */
    function initSwiper() {
        if (!window.Swiper) return;

        document.querySelectorAll(".init-swiper").forEach(function (swiperElement) {
            const cfgEl = swiperElement.querySelector(".swiper-config");
            let config = {};
            if (cfgEl) {
                try { config = JSON.parse(cfgEl.textContent.trim()); } catch { config = {}; }
            }

            // Nếu dự án có custom init với pagination riêng
            if (swiperElement.classList.contains("swiper-tab") &&
                typeof window.initSwiperWithCustomPagination === 'function') {
                window.initSwiperWithCustomPagination(swiperElement, config);
            } else {
                // Khởi tạo Swiper mặc định
                new Swiper(swiperElement, config);
            }
        });
    }
    window.addEventListener("load", initSwiper);

    /* ========== Hash offset fix on load ========== */
    window.addEventListener('load', function () {
        if (window.location.hash) {
            const section = document.querySelector(window.location.hash);
            if (section) {
                setTimeout(() => {
                    const scrollMarginTop = parseInt(getComputedStyle(section).scrollMarginTop || '0', 10);
                    window.scrollTo({
                        top: section.offsetTop - scrollMarginTop,
                        behavior: 'smooth'
                    });
                }, 100);
            }
        }
    });

    /* ========== Navmenu Scrollspy ========== */
    const navmenulinks = document.querySelectorAll('.navmenu a');
    function navmenuScrollspy() {
        const position = window.scrollY + 200;
        navmenulinks.forEach((link) => {
            if (!link.hash) return;
            const section = document.querySelector(link.hash);
            if (!section) return;

            if (position >= section.offsetTop && position <= (section.offsetTop + section.offsetHeight)) {
                document.querySelectorAll('.navmenu a.active').forEach(a => a.classList.remove('active'));
                link.classList.add('active');
            } else {
                link.classList.remove('active');
            }
        });
    }
    window.addEventListener('load', navmenuScrollspy);
    document.addEventListener('scroll', navmenuScrollspy);

})();
