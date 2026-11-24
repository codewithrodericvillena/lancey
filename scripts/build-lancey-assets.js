const puppeteer = require('../.video-tools/node_modules/puppeteer-core');
const fs = require('fs');
const path = require('path');

const ROOT = path.resolve(__dirname, '..');

const SVG_MARK = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100" width="1000" height="1000">
  <defs>
    <linearGradient id="lancey-stem" x1="0" y1="0" x2="0" y2="1">
      <stop offset="0%" stop-color="#60a5fa"/>
      <stop offset="50%" stop-color="#3b82f6"/>
      <stop offset="100%" stop-color="#1d4ed8"/>
    </linearGradient>
    <linearGradient id="lancey-tip" x1="0" y1="0" x2="1" y2="0">
      <stop offset="0%" stop-color="#00f2fe"/>
      <stop offset="100%" stop-color="#38bdf8"/>
    </linearGradient>
  </defs>
  <!-- Vertical Pillar -->
  <path d="M20 10 C20 5.58 23.58 2 28 2 L42 2 C46.42 2 50 5.58 50 10 L50 70 L76 70 C80.42 70 84 73.58 84 78 L84 82 C84 86.42 80.42 90 76 90 L28 90 C23.58 90 20 86.42 20 82 Z" fill="url(#lancey-stem)"/>
  <!-- Chisel Lance Arrowhead -->
  <polygon points="50,70 76,70 94,80 76,90 50,90" fill="url(#lancey-tip)"/>
  <polygon points="50,70 76,70 65,80 50,80" fill="rgba(255,255,255,0.3)"/>
</svg>`;

const SVG_APP_ICON = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 200 200" width="1000" height="1000">
  <defs>
    <linearGradient id="tile-bg" x1="0" y1="0" x2="1" y2="1">
      <stop offset="0%" stop-color="#1e293b"/>
      <stop offset="100%" stop-color="#080d1a"/>
    </linearGradient>
    <linearGradient id="lancey-stem" x1="0" y1="0" x2="0" y2="1">
      <stop offset="0%" stop-color="#60a5fa"/>
      <stop offset="50%" stop-color="#3b82f6"/>
      <stop offset="100%" stop-color="#1d4ed8"/>
    </linearGradient>
    <linearGradient id="lancey-tip" x1="0" y1="0" x2="1" y2="0">
      <stop offset="0%" stop-color="#00f2fe"/>
      <stop offset="100%" stop-color="#38bdf8"/>
    </linearGradient>
    <filter id="tile-shadow" x="-20%" y="-20%" width="140%" height="140%">
      <feDropShadow dx="0" dy="10" stdDeviation="12" flood-color="#1e40af" flood-opacity="0.45"/>
    </filter>
  </defs>
  <rect x="8" y="8" width="184" height="184" rx="44" fill="url(#tile-bg)"/>
  <rect x="8.5" y="8.5" width="183" height="183" rx="43.5" fill="none" stroke="rgba(255,255,255,0.16)" stroke-width="1.2"/>
  <g transform="translate(26, 24)" filter="url(#tile-shadow)">
    <path d="M30 18 C30 11.37 35.37 6 42 6 L60 6 C66.63 6 72 11.37 72 18 L72 104 L108 104 C114.63 104 120 109.37 120 116 L120 120 C120 126.63 114.63 132 108 132 L42 132 C35.37 132 30 126.63 30 120 Z" fill="url(#lancey-stem)"/>
    <polygon points="72,104 116,104 138,118 116,132 72,132" fill="url(#lancey-tip)"/>
    <polygon points="72,104 116,104 94,118 72,118" fill="rgba(255,255,255,0.3)"/>
  </g>
</svg>`;

async function renderSvgToPng(page, svgContent, width, height, outputPath) {
  const html = `<!DOCTYPE html><html><head><style>
    * { margin:0; padding:0; box-sizing:border-box; }
    body { background: transparent; overflow: hidden; width: ${width}px; height: ${height}px; display: flex; align-items: center; justify-content: center; }
    svg { width: 100%; height: 100%; display: block; }
  </style></head><body>${svgContent}</body></html>`;

  await page.setViewport({ width, height, deviceScaleFactor: 1 });
  await page.setContent(html, { waitUntil: 'load' });
  const dir = path.dirname(outputPath);
  if (!fs.existsSync(dir)) fs.mkdirSync(dir, { recursive: true });
  await page.screenshot({ path: outputPath, omitBackground: true });
  console.log('Saved:', path.relative(ROOT, outputPath));
}

(async () => {
  // 1. Write vector SVGs
  fs.writeFileSync(path.join(ROOT, 'docs/logo.svg'), SVG_APP_ICON.trim(), 'utf8');
  fs.writeFileSync(path.join(ROOT, 'site/assets/img/logo.svg'), SVG_APP_ICON.trim(), 'utf8');
  fs.writeFileSync(path.join(ROOT, 'docs/brand/lancy-logo.svg'), SVG_APP_ICON.trim(), 'utf8');
  console.log('Vector SVGs updated.');

  const browser = await puppeteer.launch({
    executablePath: 'C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe',
    headless: true,
    args: ['--no-sandbox', '--disable-setuid-sandbox']
  });
  const page = await browser.newPage();

  // 2. Render App Icon Squircle at various sizes
  await renderSvgToPng(page, SVG_APP_ICON, 1024, 1024, path.join(ROOT, 'docs/logo.png'));
  await renderSvgToPng(page, SVG_APP_ICON, 512, 512, path.join(ROOT, 'site/assets/img/icon-512.png'));
  await renderSvgToPng(page, SVG_APP_ICON, 192, 192, path.join(ROOT, 'site/assets/img/icon-192.png'));
  await renderSvgToPng(page, SVG_APP_ICON, 180, 180, path.join(ROOT, 'site/assets/img/apple-touch-icon.png'));

  // Desktop App Assets in src/Lancy.Avalonia/Assets/
  await renderSvgToPng(page, SVG_APP_ICON, 1024, 1024, path.join(ROOT, 'src/Lancy.Avalonia/Assets/app-icon-source.png'));
  await renderSvgToPng(page, SVG_APP_ICON, 512, 512, path.join(ROOT, 'src/Lancy.Avalonia/Assets/app-512.png'));
  await renderSvgToPng(page, SVG_APP_ICON, 256, 256, path.join(ROOT, 'src/Lancy.Avalonia/Assets/app-256.png'));
  await renderSvgToPng(page, SVG_APP_ICON, 64, 64, path.join(ROOT, 'src/Lancy.Avalonia/Assets/app-icon-64.png'));

  // 3. Render Header logo (64px mark, transparent)
  await renderSvgToPng(page, SVG_MARK, 64, 64, path.join(ROOT, 'site/assets/img/logo-64.png'));
  await renderSvgToPng(page, SVG_APP_ICON, 64, 64, path.join(ROOT, 'docs/logo-64.png'));

  await browser.close();
  console.log('All raster PNGs generated successfully!');
})();
