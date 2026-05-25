(function () {
  'use strict';

  // =========================================================
  // 1. THEME — runs immediately before DOM is ready to prevent
  //    flash of wrong theme
  // =========================================================
  const applyTheme = (theme) => {
    document.documentElement.setAttribute('data-theme', theme);
    localStorage.setItem('payroll-theme', theme);
    const icon = document.querySelector('.theme-icon');
    if (icon) {
      icon.className = theme === 'dark'
        ? 'fa-solid fa-sun theme-icon'
        : 'fa-solid fa-moon theme-icon';
    }
  };
  applyTheme(localStorage.getItem('payroll-theme') || 'light');

  // =========================================================
  // 2. UTILITIES
  // =========================================================

  // Debounce: prevent functions running on every keystroke
  const debounce = (fn, delay) => {
    let timer;
    return (...args) => {
      clearTimeout(timer);
      timer = setTimeout(() => fn(...args), delay);
    };
  };

  // Arabic normalizer — compiled once, reused everywhere
  const normalize = (val) =>
    (val || '').toString().toLowerCase()
      .replace(/[أإآا]/g, 'ا')
      .replace(/ة/g, 'ه')
      .replace(/ى/g, 'ي')
      .trim();

  // =========================================================
  // 3. CELEBRATION (canvas-confetti)
  // =========================================================
  window.celebrate = () => {
    if (typeof confetti === 'undefined') return;
    const end = Date.now() + 1500;
    const colors = ['#1a56db', '#7e3af2', '#0e9f6e'];
    (function frame() {
      confetti({ particleCount: 3, angle: 60,  spread: 55, origin: { x: 0 }, colors });
      confetti({ particleCount: 3, angle: 120, spread: 55, origin: { x: 1 }, colors });
      if (Date.now() < end) requestAnimationFrame(frame);
    }());
  };

  // =========================================================
  // 4. DOM READY
  // =========================================================
  document.addEventListener('DOMContentLoaded', function () {

    // A. Celebrate on success page
    if (document.querySelector('.alert-success') ||
        window.location.search.includes('paid=true')) {
      setTimeout(window.celebrate, 500);
    }

    // B. DataTables — only initialize when tables exist
    if (typeof $ !== 'undefined' && typeof $.fn.DataTable !== 'undefined') {
      const tables = document.querySelectorAll('.datatable');
      if (tables.length > 0) {
        tables.forEach(table => {
          $(table).DataTable({
            responsive: true,
            language: {
              url: 'https://cdn.datatables.net/plug-ins/1.13.7/i18n/ar.json',
              search: '_INPUT_',
              searchPlaceholder: 'ابحث هنا...',
            },
            dom: '<"d-flex flex-wrap justify-content-between align-items-center mb-3"Bf>rt<"d-flex flex-wrap justify-content-between align-items-center mt-3"ip>',
            buttons: [
              { extend: 'copy',   text: '<i class="fa-solid fa-copy me-1"></i> نسخ',      className: 'btn btn-outline-secondary btn-sm rounded-pill px-3' },
              { extend: 'excel',  text: '<i class="fa-solid fa-file-excel me-1"></i> إكسل', className: 'btn btn-success btn-sm rounded-pill px-3' },
              { extend: 'pdf',    text: '<i class="fa-solid fa-file-pdf me-1"></i> PDF',    className: 'btn btn-danger btn-sm rounded-pill px-3' },
              { extend: 'print',  text: '<i class="fa-solid fa-print me-1"></i> طباعة',    className: 'btn btn-primary btn-sm rounded-pill px-3' },
              { extend: 'colvis', text: '<i class="fa-solid fa-eye-slash me-1"></i> الأعمدة', className: 'btn btn-light btn-sm rounded-pill px-3 border' }
            ],
            pageLength: 10,
            order: [[0, 'desc']]
          });
        });
      }
    }

    // C. SweetAlert2 delete confirmation — delegated to document
    document.addEventListener('click', function (e) {
      const btn = e.target.closest('.btn-delete-confirm');
      if (!btn) return;
      e.preventDefault();
      const form = btn.closest('form');
      const entityName = btn.dataset.entity || 'هذا السجل';
      if (typeof Swal === 'undefined') { form.submit(); return; }
      Swal.fire({
        title: 'هل أنت متأكد؟',
        text: `سيتم حذف ${entityName} نهائياً!`,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#c81e1e',
        cancelButtonColor: '#718096',
        confirmButtonText: 'نعم، احذف!',
        cancelButtonText: 'إلغاء',
        reverseButtons: true
      }).then(r => { if (r.isConfirmed) form.submit(); });
    });

    // D. Theme Toggle button
    const themeToggle = document.querySelector('[data-theme-toggle]');
    if (themeToggle) {
      themeToggle.addEventListener('click', () => {
        const current = document.documentElement.getAttribute('data-theme');
        applyTheme(current === 'dark' ? 'light' : 'dark');
      });
    }

    // E. Sidebar — toggle body class only (CSS transition handles movement)
    //    No JS transforms = no layout thrashing
    const sidebarOpenBtn  = document.querySelector('[data-sidebar-open]');
    const sidebarCloseBtns = document.querySelectorAll('[data-sidebar-close]');

    if (sidebarOpenBtn) {
      sidebarOpenBtn.addEventListener('click', () => {
        document.body.classList.add('sidebar-open');
      });
    }
    sidebarCloseBtns.forEach(btn => {
      btn.addEventListener('click', () => {
        document.body.classList.remove('sidebar-open');
      });
    });

    // F. Global Search — DEBOUNCED (150ms) + cached DOM rows
    //    Prevents lag caused by querying DOM on every keystroke
    document.querySelectorAll('[data-global-search]').forEach(input => {
      let cachedRows = null; // Built once lazily on first search

      const doSearch = debounce((q) => {
        if (!cachedRows) {
          cachedRows = Array.from(
            document.querySelectorAll('tbody tr, .stack-item, .branch-bar-row')
          ).map(el => ({ el, text: normalize(el.textContent) }));
        }
        const query = normalize(q);
        // Batch DOM writes inside rAF to avoid layout thrashing
        requestAnimationFrame(() => {
          cachedRows.forEach(({ el, text }) => {
            el.classList.toggle('d-none', query.length > 0 && !text.includes(query));
          });
        });
      }, 150);

      input.addEventListener('input', e => doSearch(e.target.value));
    });

    // G. Counter animations — rAF-based, compositor-friendly
    document.querySelectorAll('[data-counter]').forEach(counter => {
      const target = Number(counter.dataset.counter);
      if (isNaN(target) || target <= 0) return;
      let frame = 0;
      const steps = 18;
      const tick = () => {
        frame++;
        counter.textContent = Math.round((target * frame) / steps).toLocaleString('ar-LY');
        if (frame < steps) requestAnimationFrame(tick);
      };
      requestAnimationFrame(tick);
    });

  });
})();
