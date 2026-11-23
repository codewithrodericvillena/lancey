#!/usr/bin/env python3
"""Draws the Lancy logo and writes every app/site icon derived from it.

Run manually (needs Pillow), then run scripts/make-brand-assets.py for the website's
rasters. Outputs are committed; CI never runs this.

    pip install pillow
    python3 scripts/make-lancy-logo.py
"""
import sys
from pathlib import Path

try:
    from PIL import Image, ImageDraw, ImageFont
except ImportError:
    sys.exit("Pillow is required: pip install pillow")

ROOT = Path(__file__).resolve().parent.parent
ASSETS = ROOT / "src" / "Lancy.Avalonia" / "Assets"
S = 2048  # draw big, downscale for clean edges
C1, C2, DOT = (99, 102, 241), (139, 92, 246), (34, 211, 238)  # indigo -> violet, cyan dot


def mark(size=S, with_tile=True):
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    if with_tile:
        grad = Image.new("RGBA", (size, size))
        px = grad.load()
        for y in range(size):
            for x in range(size):
                t = (x + y) / (2 * size)
                px[x, y] = tuple(int(C1[i] + (C2[i] - C1[i]) * t) for i in range(3)) + (255,)
        mask = Image.new("L", (size, size), 0)
        ImageDraw.Draw(mask).rounded_rectangle((0, 0, size - 1, size - 1), radius=int(size * 0.22), fill=255)
        img.paste(grad, (0, 0), mask)
    d = ImageDraw.Draw(img)
    u = size / 100
    w = 15 * u
    # the "L": stem + foot with rounded ends
    d.rounded_rectangle((30 * u, 22 * u, 30 * u + w, 76 * u), radius=w / 2, fill="white")
    d.rounded_rectangle((30 * u, 76 * u - w, 72 * u, 76 * u), radius=w / 2, fill="white")
    # accent dot: the "period" at the end of the foot
    r = 6.5 * u
    cx, cy = 72 * u + 3 * u, 76 * u - w / 2
    d.ellipse((cx - r, cy - r, cx + r, cy + r), fill=DOT)
    return img


def save_png(img, path, px):
    img.resize((px, px), Image.LANCZOS).save(path)


def main():
    full = mark()
    save_png(full, ROOT / "docs" / "logo.png", 512)
    save_png(full, ASSETS / "app-icon-source.png", 1024)
    for name, px in (("app-512.png", 512), ("app-256.png", 256), ("app-icon-64.png", 64)):
        save_png(full, ASSETS / name, px)
    sizes = [(16, 16), (24, 24), (32, 32), (48, 48), (64, 64), (128, 128), (256, 256)]
    for ico in ("app.ico", "icon.ico"):
        full.resize((256, 256), Image.LANCZOS).save(ASSETS / ico, sizes=sizes)
    full.resize((1024, 1024), Image.LANCZOS).save(ASSETS / "app.icns")

    # wordmark (transparent, white text for the dark onboarding window)
    h = 245
    word = Image.new("RGBA", (1315, h), (0, 0, 0, 0))
    word.paste(mark(h), (0, 0))
    bold = None
    for c in ("C:/Windows/Fonts/segoeuib.ttf", "C:/Windows/Fonts/arialbd.ttf",
              "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf"):
        if Path(c).exists():
            bold = ImageFont.truetype(c, 190)
            break
    ImageDraw.Draw(word).text((h + 40, h // 2), "Lancy", font=bold or ImageFont.load_default(),
                              fill=(241, 242, 246, 255), anchor="lm")
    word.save(ASSETS / "logo-wordmark.png")

    # a real SVG (the old one embedded a raster) shared by docs and the website
    svg = f'''<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100" width="512" height="512">
  <defs><linearGradient id="g" x1="0" y1="0" x2="1" y2="1">
    <stop offset="0" stop-color="rgb{C1}"/><stop offset="1" stop-color="rgb{C2}"/></linearGradient></defs>
  <rect width="100" height="100" rx="22" fill="url(#g)"/>
  <rect x="30" y="22" width="15" height="54" rx="7.5" fill="#fff"/>
  <rect x="30" y="61" width="42" height="15" rx="7.5" fill="#fff"/>
  <circle cx="75" cy="68.5" r="6.5" fill="rgb{DOT}"/>
</svg>
'''
    for p in (ROOT / "docs" / "logo.svg", ROOT / "site" / "assets" / "img" / "logo.svg"):
        p.write_text(svg, encoding="utf-8")
    print("logo + icons written")


if __name__ == "__main__":
    main()
