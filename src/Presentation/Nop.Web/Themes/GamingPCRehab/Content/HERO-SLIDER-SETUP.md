# 🎮 Hero Slider Setup Guide - Gaming PC Rehab

## How to Add Dynamic Hero Slider

The hero slider is now integrated with nopCommerce's widget system, allowing you to manage content dynamically through the admin panel.

### 📋 **Step-by-Step Setup:**

#### 1. **Access Admin Panel**
   - Login to your nopCommerce admin
   - Navigate to: **Content Management > Widgets**

#### 2. **Configure HTML Widget**
   - Find "HTML widget" in the list
   - Click **"Configure"**
   - Click **"Add New Widget"**

#### 3. **Widget Configuration**
   ```
   Widget Zone: homepage_top
   Display Order: 1
   Store: Select your store (or leave for all stores)
   Customer Roles: Leave blank (visible to all)
   ```

#### 4. **Add Hero Content**
   - Copy the HTML content from: `sample-hero-slider-widget.html`
   - Paste it into the **"Body"** field
   - Click **"Save"**

### 🎨 **Customization Options:**

#### **Edit Slider Content:**
You can modify any of these elements in the HTML widget:

```html
<!-- Change slide badge -->
<span class="hero-badge">New Arrivals</span>

<!-- Update slide title -->
<h1 class="hero-title">Your Custom <span class="highlight">Title</span></h1>

<!-- Modify description -->
<p class="hero-description">Your custom description text</p>

<!-- Update button links -->
<a href="/your-link" class="btn btn-primary">Your Button</a>

<!-- Change icons -->
<i class="fas fa-microchip hero-icon"></i>
<!-- Available icons: fa-microchip, fa-memory, fa-tools, fa-desktop, fa-gamepad -->
```

#### **Add More Slides:**
To add a 4th slide, copy this structure:

```html
<!-- Slide 4 - Your New Slide -->
<div class="hero-slide">
    <div class="hero-background">
        <div class="hero-bg-pattern"></div>
    </div>
    <div class="container">
        <div class="hero-content">
            <div class="hero-text">
                <span class="hero-badge">Your Badge</span>
                <h1 class="hero-title">Your <span class="highlight">Title</span></h1>
                <p class="hero-description">Your description text here.</p>
                <div class="hero-actions">
                    <a href="#" class="btn btn-primary">Primary Button</a>
                    <a href="#" class="btn btn-outline">Secondary Button</a>
                </div>
            </div>
            <div class="hero-image">
                <div class="hero-product-showcase">
                    <i class="fas fa-your-icon hero-icon"></i>
                </div>
            </div>
        </div>
    </div>
</div>
```

And add a dot to the controls:
```html
<span class="dot" data-slide="3"></span>
```

#### **Update Feature Benefits:**
Modify the features section:

```html
<div class="feature-item">
    <div class="feature-icon">
        <i class="fas fa-your-icon"></i>
    </div>
    <div class="feature-content">
        <h4>Your Feature</h4>
        <p>Your feature description</p>
    </div>
</div>
```

### 🔧 **Multiple Sliders:**

To create different sliders for different pages:

1. **Homepage:** Widget Zone = `homepage_top`
2. **Category Pages:** Widget Zone = `categorydetails_top`
3. **Product Pages:** Widget Zone = `productdetails_top`

### 💡 **Pro Tips:**

1. **Test First:** Use a staging environment before going live
2. **Mobile Check:** Always test on mobile devices
3. **Performance:** Keep images optimized and under 500KB
4. **SEO:** Use proper heading tags (H1) for main titles
5. **Links:** Update all links to match your actual page URLs

### 🎯 **Widget Zones Available:**

- `homepage_top` - Top of homepage (recommended for hero)
- `homepage_before_categories` - Before category section
- `homepage_before_products` - Before product section
- `left_side_column_before` - Left sidebar top
- `right_side_column_before` - Right sidebar top
- `footer` - Footer area

### 🚀 **Advanced Options:**

For more advanced users, you can:
1. Create custom ViewComponents
2. Use topic pages with HTML content
3. Integrate with third-party slider plugins
4. Add database-driven content management

### 📞 **Support:**

If you need help customizing the slider, the HTML content is fully editable through the admin panel. The CSS styling is already optimized for the gaming theme and will automatically apply to any content you add.

---
**Note:** After adding the widget, it may take a few minutes to appear due to caching. Clear your cache if changes don't appear immediately.