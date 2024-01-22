document.addEventListener('DOMContentLoaded', function () {   
    const activeClass = 'is-active';
    const sidebarClass = 'is-sidebar-hidden';
    const navbarBurger = document.getElementById('NavbarBurger');

    if (navbarBurger) {
        navbarBurger.addEventListener('click', function () {
            const mainNavbar = document.getElementById(navbarBurger.dataset.target);

            navbarBurger.classList.toggle(activeClass);
            mainNavbar.classList.toggle(activeClass);
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
        sideNavbarBurger.classList.toggle(sidebarClass);
        sideNavbar.classList.toggle(sidebarClass);
        mainContent.classList.toggle(sidebarClass);
    });
    
    window.addEventListener('resize', function () {        
        if (window.outerWidth <= 1024) {
            sideNavbarBurger.classList.add(sidebarClass);
            sideNavbar.classList.add(sidebarClass);
            mainContent.classList.add(sidebarClass);
        } else {
            sideNavbarBurger.classList.remove(sidebarClass);
            sideNavbar.classList.remove(sidebarClass);
            mainContent.classList.remove(sidebarClass);
        }
    });
});