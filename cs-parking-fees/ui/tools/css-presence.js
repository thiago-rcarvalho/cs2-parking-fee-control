/**
 * Webpack plugin to detect CSS presence and add appropriate metadata
 * Based on CS2 modding toolchain patterns
 */

class CSSPresencePlugin {
  apply(compiler) {
    compiler.hooks.emit.tapAsync('CSSPresencePlugin', (compilation, callback) => {
      const cssFiles = Object.keys(compilation.assets).filter(name => name.endsWith('.css'));
      const hasCss = cssFiles.length > 0;
      
      // Log CSS files found
      if (hasCss) {
        console.log(`📦 CSS files detected: ${cssFiles.join(', ')}`);
      }
      
      callback();
    });
  }
}

module.exports = { CSSPresencePlugin };
