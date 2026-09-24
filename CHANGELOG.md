# Release notes

## v3.0.0

### Changed
- **The package is `SPACS-Dialogs`, id `com.anotherealitysrl.spacs-dialogs`** (was `Virtuademy-SDK-Dialogs` /
  `com.anotherealitysrl.virtuademy-sdk-dialogs`). The `Virtuademy-SDK-*` prefix is kept for the SDK
  proper — Core, Environments, Library; a package that carries no platform takes the `SPACS-*`
  prefix, as SPACS-Utility did. The assemblies and namespaces were already `SPACS.Dialogs*` and are
  unchanged, so built bundles, Visual Scripting graphs and interpreted scripts are unaffected.
- Depends on `com.anotherealitysrl.spacs-graphs` 3.0.0.

### Breaking
- A project that names `com.anotherealitysrl.virtuademy-sdk-dialogs` in its `manifest.json` stops
  resolving once it pulls this version: the manifest key must match the id in `package.json`.
  Switch the key to `com.anotherealitysrl.spacs-dialogs` and the URL to `SPACS-Dialogs.git` (the package
  rename migrator in Virtuademy-SDK-Environments does both). Registry releases up to 2026.5.0 are
  unaffected: each pins the old repository URL at a tag, GitHub redirects that URL, and the tag
  still carries the id it was released with.

## v2.1.0

### Changed
- Moved initialization logic out of `Start` into a new public `Init` method, allowing explicit initialization of the dialog system.

## v2.0.0

### Added

- DialogNode: Added enum LayoutElementSide to choose wich side to place Nickname and Avatar.
- DialogNode: Added fields characterNameSide and characterAvatarSide of type LayoutElementSide.
- DialogPanelController: Added float fields to define X position deltas for Player and NPC dialogs, both for left or right side of Nickname and Avatar.

### Changed

- DialogNode: Changed fields definition order.
- DialogPanelControllerGeneric: Added new side parameters in SetNicknameText and SetAvatar methods.
- DialogPanelController: Added override method SetNicknameText to use the new X position deltas related to the chosen side, both for Player and NPC.
- DialogPanelController: Updated override method SetAvatar to use the new X position deltas related to the chosen side, both for Player and NPC.
- DialogPanelControllerEditor: Adapted to new fields.
- DialogPanel.prefab: Added default X position deltas for nicknames and avatars of Player and NPC boxes.

## v1.0.0

- Initial release
