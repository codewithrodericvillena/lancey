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


def _l_shape(size):
    """White "L" whose foot is a blade tapering to a lance point. Returned as an L-mode mask."""
    m = Image.new("L", (size, size), 0)
    d = ImageDraw.Draw(m)
    u = size / 100
    w = 15 * u
    x0, top, bottom = 28 * u, 19 * u, 79 * u
    d.rounded_rectangle((x0, top, x0 + w, bottom), radius=w / 2, fill=255)                       # stem
    d.polygon([(x0 + w / 2, bottom - w * 1.18), (89 * u, bottom - w * 0.42), (x0 + w / 2, bottom)], fill=255)  # blade
    return m


def mark(size=S, with_tile=True):
    from PIL import ImageFilter
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    if with_tile:
        # indigo -> violet diagonal, with a cooler glow bottom-right
        grad = Image.new("RGBA", (size, size))
        px = grad.load()
        stops = [(0.0, (79, 70, 229)), (0.55, (124, 92, 246)), (1.0, (168, 85, 247))]
        for y in range(size):
            for x in range(size):
                t = (x + y) / (2 * size)
                for (t0, c0), (t1, c1) in zip(stops, stops[1:]):
                    if t <= t1:
                        k = (t - t0) / (t1 - t0)
                        px[x, y] = tuple(int(c0[i] + (c1[i] - c0[i]) * k) for i in range(3)) + (255,)
                        break
        mask = Image.new("L", (size, size), 0)
        ImageDraw.Draw(mask).rounded_rectangle((0, 0, size - 1, size - 1), radius=int(size * 0.225), fill=255)
        # soft top-left sheen
        sheen = Image.new("L", (size, size), 0)
        ImageDraw.Draw(sheen).ellipse((-size * 0.3, -size * 0.45, size * 0.85, size * 0.55), fill=34)
        sheen = sheen.filter(ImageFilter.GaussianBlur(size * 0.08))
        white = Image.new("RGBA", (size, size), (255, 255, 255, 255))
        grad = Image.composite(white, grad, sheen)
        img.paste(grad, (0, 0), mask)
    lmask = _l_shape(size)
    # drop shadow under the L
    shadow = Image.new("RGBA", (size, size), (30, 20, 90, 0))
    shadow.putalpha(lmask.point(lambda v: int(v * 0.45)))
    shadow = shadow.filter(ImageFilter.GaussianBlur(size * 0.018))
    shifted = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    shifted.paste(shadow, (0, int(size * 0.018)))
    img = Image.alpha_composite(img, shifted)
    # the L itself: white with a faint vertical fade for depth
    letter = Image.new("RGBA", (size, size), (255, 255, 255, 255))
    fade = Image.linear_gradient("L").resize((size, size)).point(lambda v: 255)
    letter.putalpha(Image.composite(lmask, Image.new("L", (size, size), 0), fade))
    img = Image.alpha_composite(img, letter)
    # blade tint: white -> cyan toward the point, only beyond the stem
    u = size / 100
    tint = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    tp = tint.load()
    x_start, x_end = 43 * u, 89 * u
    for x in range(int(x_start), min(size, int(x_end) + 1)):
        k = (x - x_start) / (x_end - x_start)
        col = tuple(int(255 + (DOT[i] - 255) * k) for i in range(3)) + (255,)
        for y in range(int(55 * u), int(80 * u)):
            tp[x, y] = col
    tint.putalpha(Image.composite(lmask, Image.new("L", (size, size), 0), tint.getchannel("A")))
    img = Image.alpha_composite(img, tint)
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
  <defs>
    <linearGradient id="g" x1="0" y1="0" x2="1" y2="1">
      <stop offset="0" stop-color="rgb(79,70,229)"/><stop offset=".55" stop-color="rgb(124,92,246)"/><stop offset="1" stop-color="rgb(168,85,247)"/>
    </linearGradient>
    <filter id="s" x="-20%" y="-20%" width="140%" height="140%"><feDropShadow dx="0" dy="1.8" stdDeviation="1.6" flood-color="rgb(30,20,90)" flood-opacity=".45"/></filter>
  </defs>
  <rect width="100" height="100" rx="22.5" fill="url(#g)"/>
  <linearGradient id="b" x1="43" y1="0" x2="89" y2="0" gradientUnits="userSpaceOnUse">
    <stop offset="0" stop-color="#fff"/><stop offset="1" stop-color="rgb{DOT}"/>
  </linearGradient>
  <g filter="url(#s)">
    <rect x="28" y="19" width="15" height="60" rx="7.5" fill="#fff"/>
    <polygon points="35.5,61.3 89,72.6 35.5,79" fill="url(#b)"/>
  </g>
</svg>
'''
    for p in (ROOT / "docs" / "logo.svg", ROOT / "site" / "assets" / "img" / "logo.svg"):
        p.write_text(svg, encoding="utf-8")
    print("logo + icons written")


if __name__ == "__main__":
    main()
