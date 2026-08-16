# Phase 2D Theme Resource Key Manifest

> Status: Active handoff manifest. Keep synchronized with ThemeResources.xaml and PriorityBrushProvider.cs.

## Resource Keys

| Key | Purpose | Hex / Value |
|---|---|---|
| TaskTreePriorityCriticalBrush | Critical priority border/accent | #D13438 |
| TaskTreePriorityHighBrush | High priority border/accent | #FF8C00 |
| TaskTreePriorityNormalBrush | Normal priority border/accent | #FFD700 |
| TaskTreePriorityLowBrush | Low priority border/accent | #4F9DE8 |
| TaskTreePriorityTrivialBrush | Trivial priority border/accent | #8A8886 |
| TaskTreeWindowBackgroundBrush | Main window background | #F7F7F7 |
| TaskTreePanelBackgroundBrush | Panel/toast surface | #FFFFFF |
| TaskTreePanelBorderBrush | Panel border | #D0D0D0 |
| TaskTreeMutedTextBrush | Secondary text | #666666 |
| TaskTreeForegroundBrush | Primary text and control foreground | #202020 light / #F2F2F2 dark |
| TaskTreeInputBackgroundBrush | TextBox/ComboBox surface | #FFFFFF light / #353535 dark |
| TaskTreeInputBorderBrush | TextBox/ComboBox border | #B8B8B8 light / #707070 dark |
| TaskTreePanelPadding | Common panel padding | 12 |
| TaskTreePanelCornerRadius | Common corner radius | 8 |

## Codex/Claude Notes

- Gap #143: PriorityBrushProvider and ThemeResources must stay synchronized until a true WPF resource lookup/converter is introduced.
- Gap #147: Phase 5C must load ThemeResources.xaml in a real WPF Application context and verify all keys resolve.
- Runtime theme application now swaps the app-level light/dark resource dictionaries from the existing `ThemePreference`; `System` follows the Windows `AppsUseLightTheme` setting and falls back to light when unavailable.
