# Attribution and modification notice

This is an independent adaptation of **FF14-Kefka-P4-for-PC / UMADOverlay** by
**AweiYourdog**:

https://github.com/AweiYourdog/FF14-Kefka-P4-for-PC

Upstream source used: `9d8598f193e4d74b853b0f948ecde321b7a7e697`.

The original GNU General Public License version 3 text is retained in `LICENSE`.
This adapted project is distributed under GPL-3.0. It is not an official
upstream release. The original README is retained as `UPSTREAM-README.md` for
provenance; its feature and executable safety claims do not describe this build.

Changes made **2026-10-05** for Kinson's requested personal layout:

- Replaced the single combined window with a trigger window and result window.
- Added one 48-DIP launcher that hides/restores both overlays together.
- Arranged the 18 Flow inputs into five columns and five rows, with a blank
  centre column, following the proportions of the supplied hotbar reference.
- Added independent dragging/resizing, layout locking, window recovery,
  persistent preferences, and paired visibility management.
- Converted the three original WebP icons to PNG without redrawing them.
- Retained the original Flow mechanic engine, translation dictionary and
  command implementation. Removed the old mode-specific UI from this build.
- Added regression tests, WPF smoke tests, a Windows build workflow and build
  instructions.

Existing author notices and the original licence are preserved. No ownership
of the upstream work or third-party game assets is claimed. This software is
provided without warranty; see `LICENSE`.

Revision **2026-10-06** (Hong Kong): restored the original vertical Flow ordering,
blue circles/red question marks, water-left/fire-right placement, original labels
and section headings. The reference screenshot now governs only window size
and proportions. Increased contrast with opaque black result rows, white labels
and yellow answers, and upgraded existing translucent preferences once.
