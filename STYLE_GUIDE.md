# Hotel Manager - UI/UX Style Guide

## 📖 Overview

This style guide defines the visual language and design principles for the Hotel Manager application, ensuring consistency across all UI components and views.

## 🎨 Design Philosophy

### Material Design 3 Principles
- **Clean & Modern**: Simplified interfaces with purposeful use of space
- **Consistent**: Unified visual language across all components
- **Accessible**: High contrast ratios and readable typography
- **Responsive**: Adaptive layouts for different screen sizes
- **Intuitive**: Clear navigation and user-friendly interactions

## 🎨 Color System

### Primary Colors
```xml
PrimaryBrush: #6200EA (Purple 700)
Primary100Brush: #E8E1FF (Light Purple)
Primary200Brush: #C8B5FF
Primary500Brush: #8C5CF6  
Primary700Brush: #6200EA
```

### Secondary Colors
```xml
SecondaryBrush: #03DAC5 (Teal A200)
Secondary100Brush: #F0FDFC
Secondary500Brush: #03DAC5
```

### Surface & Background
```xml
SurfaceBrush: #FFFFFF
BackgroundBrush: #FAFAFA
SurfaceGradient: Linear gradient from #F8FAFC to #F1F5F9
PrimaryGradient: Linear gradient from #667eea to #764ba2
```

### Semantic Colors
```xml
SuccessBrush: #10B981 (Green)
Success100Brush: #ECFDF5
WarningBrush: #F59E0B (Amber)
Warning100Brush: #FFFBEB
ErrorBrush: #EF4444 (Red)
Error100Brush: #FEF2F2
```

### Text Colors
```xml
OnPrimaryBrush: #FFFFFF
OnPrimary200Brush: #E5E7EB
OnSurfaceBrush: #1F2937
OnSurfaceVariantBrush: #6B7280
```

## 📝 Typography

### Font Family
- **Primary**: Segoe UI (Windows), San Francisco (macOS), Roboto (fallback)

### Text Styles
```xml
<!-- Headlines -->
HeadlineLarge: 32px, Bold
HeadlineMedium: 28px, SemiBold  
HeadlineSmall: 24px, SemiBold

<!-- Titles -->
TitleLarge: 22px, Medium
TitleMedium: 16px, Medium

<!-- Body Text -->
BodyLarge: 16px, Regular
BodyMedium: 14px, Regular
BodySmall: 12px, Regular

<!-- Labels -->
LabelLarge: 14px, Medium
LabelMedium: 12px, Medium
```

## 🎯 Spacing System

### Standard Spacing Values
```xml
SpacingXS: 4px
SpacingS: 8px  
SpacingM: 16px
SpacingL: 24px
SpacingXL: 32px
SpacingXXL: 48px
```

### Margins
```xml
MarginXS: 4px
MarginS: 8px
MarginM: 16px  
MarginL: 24px
MarginXL: 32px
```

## 🔲 Component Styles

### Buttons

#### Primary Button (ContainedButton)
- Background: PrimaryBrush
- Foreground: White
- Border Radius: 8px
- Padding: 12px 24px
- Elevation: 2

#### Secondary Button (OutlinedButton)  
- Background: Transparent
- Foreground: PrimaryBrush
- Border: 1px PrimaryBrush
- Border Radius: 8px
- Padding: 12px 24px

#### Text Button (TextButton)
- Background: Transparent
- Foreground: PrimaryBrush
- No border
- Padding: 8px 16px

#### Icon Button (IconButton)
- Size: 40x40px
- Background: Transparent
- Hover: Light background overlay

### Cards

#### Material Design Card
- Background: SurfaceBrush
- Border Radius: 12px
- Elevation: 1-3 levels
- Padding: 16-24px

#### Quick Stats Card
- Hover elevation increase
- Icon + Text layout
- Consistent padding: 16px

### Form Controls

#### Text Fields (OutlinedTextField)
- Border: 1px OnSurfaceVariantBrush
- Border Radius: 8px
- Padding: 12px 16px
- Focus: PrimaryBrush border

#### ComboBox (MaterialDesignOutlinedComboBox)
- Consistent with text fields
- Dropdown icon styling
- Hint text support

#### DatePicker (MaterialDesignDatePicker)
- Calendar icon
- Consistent styling with text fields

### Data Grid (ModernDataGrid)

#### Table Structure
- Header: SurfaceBrush background
- Alternating row colors
- Border: OnSurfaceVariantBrush
- Row height: 48px minimum

#### Headers (ModernDataGridColumnHeader)
- Font: LabelLarge
- Padding: 16px 12px
- Sort indicators

## 📐 Layout Patterns

### Page Structure
```
Header (80px height)
├── Title + Subtitle
└── Action Buttons (Logout, etc.)

Content Area (ScrollViewer)
├── Quick Stats Grid
├── Form Sections (Cards)
└── Data Lists (Cards)
```

### Card Layout
- Section header with icon + title
- Form grids with consistent spacing
- Action buttons aligned right

### Navigation
- Material Design drawer/sidebar
- Clear hierarchy
- Active state indicators

## 🎯 Icon Guidelines

### Icon Library
- Material Design Icons
- 16px, 20px, 24px standard sizes
- Consistent stroke width
- Semantic color usage

### Common Icons
- Add: Plus
- Edit: Pencil  
- Delete: Delete
- View: Eye
- Search: Magnify
- Logout: Logout
- Check: Check/CheckCircle
- Error: Alert/AlertCircle

## 🎭 Animation & Transitions

### Elevation Changes
- Duration: 0.2s
- Easing: ease-out

### Page Transitions
- Duration: 0.3s
- Slide/fade combinations

### Loading States
- Progress indicators
- Skeleton loaders for data

## ♿ Accessibility

### Color Contrast
- Minimum 4.5:1 for normal text
- Minimum 3:1 for large text
- High contrast mode support

### Keyboard Navigation
- Tab order logical
- Enter/Space activation
- Escape for dismissal

### Screen Reader Support
- Meaningful labels
- ARIA attributes
- Role definitions

## 📱 Responsive Design

### Breakpoints
- Small: < 768px
- Medium: 768px - 1024px  
- Large: > 1024px

### Adaptive Elements
- Grid columns adjust
- Side panels collapse
- Text scaling

## 🔧 Implementation Guidelines

### Resource Organization
```
Styles/
├── Themes/
│   ├── Colors.xaml
│   ├── Typography.xaml
│   ├── Spacing.xaml
│   └── Components.xaml
└── AppStyles.xaml (main merger)
```

### Naming Conventions
- Colors: `{Color}{Variant}Brush` (e.g., Primary500Brush)
- Spacing: `{Type}{Size}` (e.g., SpacingL, MarginM)
- Styles: `{Component}{Variant}` (e.g., ContainedButton)

### Code Examples

#### Using Color Resources
```xml
<Button Background="{StaticResource PrimaryBrush}"
        Foreground="{StaticResource OnPrimaryBrush}"/>
```

#### Applying Typography
```xml
<TextBlock Text="Headline" 
           Style="{StaticResource HeadlineLarge}"/>
```

#### Card Implementation
```xml
<materialDesign:Card Style="{StaticResource QuickStatsCard}">
    <StackPanel Margin="{StaticResource SpacingM}">
        <!-- Content -->
    </StackPanel>
</materialDesign:Card>
```

## 🎯 Best Practices

### Do's
✅ Use consistent spacing values  
✅ Follow Material Design principles  
✅ Maintain proper contrast ratios  
✅ Use semantic color naming  
✅ Group related elements in cards  
✅ Provide clear visual hierarchy  

### Don'ts
❌ Mix different design systems  
❌ Use hardcoded colors/spacing  
❌ Ignore accessibility guidelines  
❌ Create inconsistent layouts  
❌ Overcomplicate navigation  

## 🔄 Version History

- **v1.0** (Phase 4): Initial Material Design 3 implementation
- Core color system and typography
- Component library establishment
- Responsive layout patterns

---

*This style guide is a living document that evolves with the application. Always refer to the latest version for current standards.* 