document.addEventListener('DOMContentLoaded', function () {        
    const navbarBurger = document.getElementById('NavbarBurger');

    if (navbarBurger) {
        navbarBurger.addEventListener('click', function () {
            const mainNavbar = document.getElementById(navbarBurger.dataset.target);

            navbarBurger.classList.toggle('is-active');
            mainNavbar.classList.toggle('is-active');
        });
    }
    
    const sideNavbarBurger = document.getElementById('SideNavbarBurger');

    if (!sideNavbarBurger) {
        return;
    }

    const sideNavbar = document.getElementById(sideNavbarBurger.dataset.target);

    if (!sideNavbar) {
        return;
    }

    const mainContent = document.getElementById(sideNavbar.dataset.target);

    if (!mainContent) {
        return;
    }
    
    sideNavbarBurger.addEventListener('click', function () {        
        sideNavbarBurger.classList.toggle('is-sidebar-hidden');
        sideNavbar.classList.toggle('is-sidebar-hidden');
        mainContent.classList.toggle('is-sidebar-hidden');
    });
    
    window.addEventListener('resize', function () {        
        if (window.outerWidth <= 1024) {
            sideNavbarBurger.classList.add('is-sidebar-hidden');
            sideNavbar.classList.add('is-sidebar-hidden');
            mainContent.classList.add('is-sidebar-hidden');
        } else {
            sideNavbarBurger.classList.remove('is-sidebar-hidden');
            sideNavbar.classList.remove('is-sidebar-hidden');
            mainContent.classList.remove('is-sidebar-hidden');
        }
    });
});