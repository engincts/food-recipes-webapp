/**
* vue v3.4.35
* (c) 2018-present Yuxi (Evan) You and Vue contributors
* @license MIT
**/
'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

var compilerDom = require('@vue/compiler-dom');
var runtimeDom = require('@vue/runtime-dom');
var shared = require('@vue/shared');

function _interopNamespaceDefault(e) {
  var n = Object.create(null);
  if (e) {
    for (var k in e) {
      n[k] = e[k];
    }
  }
  n.default = e;
  return Object.freeze(n);
}

var runtimeDom__namespace = /*#__PURE__*/_interopNamespaceDefault(runtimeDom);

const compileCache = /* @__PURE__ */ new WeakMap();
function getCache(options) {
  let c = compileCache.get(options != null ? options : shared.EMPTY_OBJ);
  if (!c) {
    c = /* @__PURE__ */ Object.create(null);
    compileCache.set(options != null ? options : shared.EMPTY_OBJ, c);
  }
  return c;
}
function compileToFunction(template, options) {
  if (!shared.isString(template)) {
    if (template.nodeType) {
      template = template.innerHTML;
    } else {
      runtimeDom.warn(`invalid template option: `, template);
      return shared.NOOP;
    }
  }
  const key = template;
  const cache = getCache(options);
  const cached = cache[key];
  if (cached) {
    return cached;
  }
  if (template[0] === "#") {
    const el = document.querySelector(template);
    if (!el) {
      runtimeDom.warn(`Template element not found or is empty: ${template}`);
    }
    template = el ? el.innerHTML : ``;
  }
  const opts = shared.extend(
    {
      hoistStatic: true,
      onError: onError ,
      onWarn: (e) => onError(e, true) 
    },
    options
  );
  if (!opts.isCustomElement && typeof customElements !== "undefined") {
    opts.isCustomElement = (tag) => !!customElements.get(tag);
  }
  const { code } = compilerDom.compile(template, opts);
  function onError(err, asWarning = false) {
    const message = asWarning ? err.message : `Template compilation error: ${err.message}`;
    const codeFrame = err.loc && shared.generateCodeFrame(
      template,
      err.loc.start.offset,
      err.loc.end.offset
    );
    runtimeDom.warn(codeFrame ? `${message}
${codeFrame}` : message);
  }
  const render = new Function("Vue", code)(runtimeDom__namespace);
  render._rc = true;
  return cache[key] = render;
}
runtimeDom.registerRuntimeCompiler(compileToFunction);

exports.compile = compileToFunction;
Object.keys(runtimeDom).forEach(function (k) {
  if (k !== 'default' && !Object.prototype.hasOwnProperty.call(exports, k)) exports[k] = runtimeDom[k];
});
