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
| TaskTreePanelPadding | Common panel padding | 12 |
| TaskTreePanelCornerRadius | Common corner radius | 8 |

## Codex/Claude Notes

- Gap #143: PriorityBrushProvider and ThemeResources must stay synchronized until a true WPF resource lookup/converter is introduced.
- Gap #147: Phase 5C must load ThemeResources.xaml in a real WPF Application context and verify all keys resolve.
