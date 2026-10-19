import fs from 'node:fs';
import path from 'node:path';

function getDirSize(dirPath) {
  let total = 0;
  try {
    const entries = fs.readdirSync(dirPath, { withFileTypes: true });
    for (const entry of entries) {
      const fullPath = path.join(dirPath, entry.name);
      try {
        if (entry.isDirectory()) {
          total += getDirSize(fullPath);
        } else if (entry.isFile()) {
          total += fs.statSync(fullPath).size;
        }
      } catch (e) {}
    }
  } catch (e) {}
  return total;
}

function analyzeDir(targetDir) {
  const items = fs.readdirSync(targetDir, { withFileTypes: true });
  const results = [];
  for (const item of items) {
    const fullPath = path.join(targetDir, item.name);
    let size = 0;
    try {
      if (item.isDirectory()) {
        size = getDirSize(fullPath);
      } else {
        size = fs.statSync(fullPath).size;
      }
    } catch (e) {}
    results.push({ name: item.name, isDir: item.isDirectory(), sizeBytes: size, sizeMB: (size / (1024 * 1024)).toFixed(2) });
  }
  results.sort((a, b) => b.sizeBytes - a.sizeBytes);
  console.log(`--- BREAKDOWN FOR: ${targetDir} ---`);
  for (const r of results) {
    const typeStr = r.isDir ? '[DIR]' : '[FILE]';
    console.log(`${typeStr.padEnd(7)} ${r.name.padEnd(45)} ${r.sizeMB.padStart(10)} MB`);
  }
}

analyzeDir(path.join(process.cwd(), 'app'));
analyzeDir(path.join(process.cwd(), 'app', 'src-tauri'));
