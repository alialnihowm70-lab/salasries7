(function () {
  // 1. Theme Management (Immediate Execution)
  const applyTheme = (theme) => {
    document.documentElement.setAttribute('data-theme', theme);
    localStorage.setItem('payroll-theme', theme);
    const icon = document.querySelector('.theme-icon');
    if (icon) {
      icon.className = theme === 'dark' ? 'fa-solid fa-sun theme-icon' : 'fa-solid fa-moon theme-icon';
    }
  };

  const initialTheme = localStorage.getItem('payroll-theme') || 'light';
  applyTheme(initialTheme);

  // 1b. Celebration Effect
  window.celebrate = () => {
    const end = Date.now() + (1.5 * 1000);
    const colors = ['#1a56db', '#7e3af2', '#0e9f6e'];

    (function frame() {
      confetti({
        particleCount: 3,
        angle: 60,
        spread: 55,
        origin: { x: 0 },
        colors: colors
      });
      confetti({
        particleCount: 3,
        angle: 120,
        spread: 55,
        origin: { x: 1 },
        colors: colors
      });

      if (Date.now() < end) {
        requestAnimationFrame(frame);
      }
    }());
  };

  $(document).ready(function () {
    // Check for success messages to celebrate
    if ($('.alert-success').length > 0 || window.location.search.includes('paid=true')) {
      setTimeout(window.celebrate, 500);
    }
    // A. Initialize DataTables
    if ($('.datatable').length > 0) {
      $('.datatable').each(function () {
        $(this).DataTable({
          responsive: true,
          language: {
            url: 'https://cdn.datatables.net/plug-ins/1.13.7/i18n/ar.json',
            search: "_INPUT_",
            searchPlaceholder: "ابحث هنا...",
          },
          dom: '<"d-flex flex-wrap justify-content-between align-items-center mb-3"Bf>rt<"d-flex flex-wrap justify-content-between align-items-center mt-3"ip>',
          buttons: [
            { extend: 'copy', text: '<i class="fa-solid fa-copy me-1"></i> نسخ', className: 'btn btn-outline-secondary btn-sm rounded-pill px-3' },
            { extend: 'excel', text: '<i class="fa-solid fa-file-excel me-1"></i> إكسل', className: 'btn btn-success btn-sm rounded-pill px-3 shadow-sm' },
            { extend: 'pdf', text: '<i class="fa-solid fa-file-pdf me-1"></i> PDF', className: 'btn btn-danger btn-sm rounded-pill px-3 shadow-sm' },
            { extend: 'print', text: '<i class="fa-solid fa-print me-1"></i> طباعة', className: 'btn btn-primary btn-sm rounded-pill px-3 shadow-sm' },
            { extend: 'colvis', text: '<i class="fa-solid fa-eye-slash me-1"></i> الأعمدة', className: 'btn btn-light btn-sm rounded-pill px-3 border' }
          ],
          pageLength: 10,
          order: [[0, 'desc']]
        });
      });
    }

    // B. Global SweetAlert2
    $(document).on('click', '.btn-delete-confirm', function (e) {
      e.preventDefault();
      const form = $(this).closest('form');
      const entityName = $(this).data('entity') || 'هذا السجل';

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
      }).then((result) => {
        if (result.isConfirmed) form.submit();
      });
    });

    // C. Theme Toggle Listener
    const themeToggle = document.querySelector('[data-theme-toggle]');
    if (themeToggle) {
      themeToggle.addEventListener('click', () => {
        const currentTheme = document.documentElement.getAttribute('data-theme');
        applyTheme(currentTheme === 'dark' ? 'light' : 'dark');
      });
    }

    // D. Sidebar Management
    const sidebarOpen = document.querySelector('[data-sidebar-open]');
    const sidebarClose = document.querySelectorAll('[data-sidebar-close]');
    const sidebar = document.getElementById('appSidebar');
    const backdrop = document.querySelector('.sidebar-backdrop');

    if (sidebarOpen && sidebar) {
      sidebarOpen.addEventListener('click', () => {
        sidebar.classList.add('active');
        backdrop?.classList.add('active');
      });
    }

    sidebarClose.forEach(btn => {
      btn.addEventListener('click', () => {
        sidebar?.classList.remove('active');
        backdrop?.classList.remove('active');
      });
    });

    // E. Search & Filtering
    const normalize = (val) => (val || "").toString().toLowerCase().replace(/[أإآا]/g, "ا").replace(/[ة]/g, "ه").replace(/[ى]/g, "ي").trim();

    document.querySelectorAll("[data-global-search]").forEach(input => {
      input.addEventListener("input", (e) => {
        const q = normalize(e.target.value);
        document.querySelectorAll("tbody tr, .stack-item, .branch-bar-row").forEach(item => {
          const match = !q || normalize(item.textContent).includes(q);
          item.classList.toggle("d-none", !match);
        });
      });
    });

    // F. Animate Counters
    document.querySelectorAll("[data-counter]").forEach(counter => {
      const target = Number(counter.dataset.counter);
      if (isNaN(target) || target <= 0) return;
      let frame = 0;
      const steps = 20;
      const tick = () => {
        frame++;
        counter.textContent = Math.round((target * frame) / steps).toLocaleString("ar-LY");
        if (frame < steps) requestAnimationFrame(tick);
      };
      requestAnimationFrame(tick);
    });
  });
})();
