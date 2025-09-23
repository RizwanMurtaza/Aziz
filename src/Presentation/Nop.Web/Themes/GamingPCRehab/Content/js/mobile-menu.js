// Gaming PC Rehab - Mobile Menu JavaScript
document.addEventListener('DOMContentLoaded', function() {
    // Create mobile menu HTML if it doesn't exist
    if (!document.querySelector('.mobile-menu-toggle')) {
        const headerLower = document.querySelector('.header-lower .container');
        if (headerLower) {
            // Create mobile menu button
            const mobileMenuBtn = document.createElement('button');
            mobileMenuBtn.className = 'mobile-menu-toggle';
            mobileMenuBtn.innerHTML = '<i class="fa fa-bars"></i>';
            mobileMenuBtn.setAttribute('aria-label', 'Toggle mobile menu');
            headerLower.appendChild(mobileMenuBtn);

            // Create mobile sidebar
            const mobileSidebar = document.createElement('div');
            mobileSidebar.className = 'mobile-sidebar-menu';
            mobileSidebar.innerHTML = `
                <div class="mobile-sidebar-header">
                    <span class="logo-text">MENU</span>
                    <button class="mobile-sidebar-close" aria-label="Close menu"><i class="fa fa-times"></i></button>
                </div>
                <div class="mobile-search-container">
                    <form class="search-form" action="/search" method="get">
                        <input type="text" name="q" class="search-input" placeholder="Search products...">
                        <button type="submit" class="search-btn" aria-label="Search"><i class="fa fa-search"></i></button>
                    </form>
                </div>
                <ul class="mobile-nav-list">
                    <li><a href="/"><i class="fa fa-home"></i> Home</a></li>
                    <li><a href="/catalog"><i class="fa fa-th-large"></i> Categories</a></li>
                    <li><a href="/newproducts"><i class="fa fa-star"></i> New Products</a></li>
                    <li><a href="/manufacturers/all"><i class="fa fa-industry"></i> Brands</a></li>
                    <li><a href="/cart"><i class="fa fa-shopping-cart"></i> Cart</a></li>
                    <li><a href="/customer/info"><i class="fa fa-user"></i> My Account</a></li>
                    <li><a href="/wishlist"><i class="fa fa-heart"></i> Wishlist</a></li>
                    <li><a href="/contactus"><i class="fa fa-envelope"></i> Contact Us</a></li>
                </ul>
            `;
            document.body.appendChild(mobileSidebar);

            // Create overlay
            const overlay = document.createElement('div');
            overlay.className = 'mobile-nav-overlay';
            document.body.appendChild(overlay);

            // Toggle menu function
            function toggleMobileMenu() {
                const isActive = mobileSidebar.classList.contains('active');

                if (isActive) {
                    // Close menu
                    mobileSidebar.classList.remove('active');
                    overlay.classList.remove('active');
                    document.body.style.overflow = '';
                    mobileMenuBtn.setAttribute('aria-expanded', 'false');
                } else {
                    // Open menu
                    mobileSidebar.classList.add('active');
                    overlay.classList.add('active');
                    document.body.style.overflow = 'hidden';
                    mobileMenuBtn.setAttribute('aria-expanded', 'true');
                }
            }

            // Toggle menu on button click
            mobileMenuBtn.addEventListener('click', function(e) {
                e.stopPropagation();
                toggleMobileMenu();
            });

            // Close menu on close button click
            const closeBtn = mobileSidebar.querySelector('.mobile-sidebar-close');
            if (closeBtn) {
                closeBtn.addEventListener('click', function() {
                    toggleMobileMenu();
                });
            }

            // Close on overlay click
            overlay.addEventListener('click', function() {
                if (mobileSidebar.classList.contains('active')) {
                    toggleMobileMenu();
                }
            });

            // Close menu on escape key
            document.addEventListener('keydown', function(e) {
                if (e.key === 'Escape' && mobileSidebar.classList.contains('active')) {
                    toggleMobileMenu();
                }
            });

            // Prevent menu close when clicking inside the sidebar
            mobileSidebar.addEventListener('click', function(e) {
                e.stopPropagation();
            });

            // Handle window resize
            let resizeTimer;
            window.addEventListener('resize', function() {
                clearTimeout(resizeTimer);
                resizeTimer = setTimeout(function() {
                    // Close mobile menu if window is resized to desktop size
                    if (window.innerWidth > 991 && mobileSidebar.classList.contains('active')) {
                        toggleMobileMenu();
                    }
                }, 250);
            });
        }
    }

    // Add touch event support for better mobile experience
    if ('ontouchstart' in window) {
        document.querySelectorAll('.mobile-nav-list a').forEach(function(link) {
            link.addEventListener('touchstart', function() {
                this.style.background = 'rgba(255, 107, 53, 0.2)';
            });
            link.addEventListener('touchend', function() {
                this.style.background = '';
            });
        });
    }
});