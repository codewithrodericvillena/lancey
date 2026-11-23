#!/usr/bin/env bash
# Lancy - .deb package builder
#
# Prerequisites: .NET 8 SDK, dpkg-deb (standard on Debian/Ubuntu)
#
# Usage: ./build-deb.sh

set -euo pipefail

VERSION="2.0.0"
RID="linux-x64"
PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../../src/Lancy.Avalonia" && pwd)"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DEBIAN_ROOT="${SCRIPT_DIR}/debian"
BIN_DEST="${DEBIAN_ROOT}/usr/bin/lancy-bin"

echo "==> Publishing self-contained ${RID} build..."
rm -rf "${BIN_DEST}"
mkdir -p "${BIN_DEST}"
dotnet publish "${PROJECT_DIR}/Lancy.Avalonia.csproj" \
  -c Release -r "${RID}" --self-contained true \
  -p:PublishSingleFile=true \
  -o "${BIN_DEST}"
chmod +x "${BIN_DEST}/Lancy"

# Convenience symlink so `lancy` works from a terminal too.
mkdir -p "${DEBIAN_ROOT}/usr/bin"
ln -sf "lancy-bin/Lancy" "${DEBIAN_ROOT}/usr/bin/lancy"

ICON_SRC="${PROJECT_DIR}/Assets/app-256.png"
if [ -f "${ICON_SRC}" ]; then
  # mkdir -p here matters: git never tracks empty directories, so a fresh
  # clone of this repo has no usr/share/icons/... tree at all - this exact
  # gap is what broke the first CI run of this script (cp failed with "No
  # such file or directory" on a clean checkout that worked fine locally,
  # where the empty directory happened to still be sitting on disk).
  mkdir -p "${DEBIAN_ROOT}/usr/share/icons/hicolor/256x256/apps"
  cp "${ICON_SRC}" "${DEBIAN_ROOT}/usr/share/icons/hicolor/256x256/apps/lancy.png"
else
  echo "WARNING: ${ICON_SRC} not found - package will have no icon."
fi

echo "==> Building .deb..."
dpkg-deb --build --root-owner-group "${DEBIAN_ROOT}" "${SCRIPT_DIR}/lancy_${VERSION}_amd64.deb"

echo "==> Done: ${SCRIPT_DIR}/lancy_${VERSION}_amd64.deb"
echo "    Install with: sudo dpkg -i lancy_${VERSION}_amd64.deb"
