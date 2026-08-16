(() => {
  const REPO = "RossWright/RWML";
  const BLOB = `https://github.com/${REPO}/blob/main/`;
  const TREE = `https://api.github.com/repos/${REPO}/git/trees/main?recursive=1`;
  const LIBRARY_ORDER = [
    "MetalCore",
    "MetalChain",
    "MetalInjection",
    "MetalCommand",
    "MetalNexus",
    "MetalGuardian"
  ];
  const SEED = [
    "README.md",
    "LICENSE.md",
    "Docs/recipes/README.md",
    "Docs/api-index/README.md"
  ];
  const LANG = {
    "c#": "csharp",
    cs: "csharp",
    "c-sharp": "csharp",
    shell: "bash",
    sh: "bash",
    zsh: "bash",
    yml: "yaml"
  };

  const navEl = document.getElementById("nav");
  const contentEl = document.getElementById("content");
  const tocEl = document.getElementById("toc");
  const editEl = document.getElementById("edit-link");
  const overlay = document.getElementById("search-overlay");
  const searchInput = document.getElementById("search-input");
  const searchResults = document.getElementById("search-results");
  const backdrop = document.getElementById("backdrop");
  const navToggle = document.querySelector(".nav-toggle");

  const cache = new Map();
  const titles = new Map();
  let paths = [];
  let searchIndex = [];
  let searchHits = [];
  let searchCursor = 0;

  marked.use({ gfm: true, breaks: false });

  function isPublicDoc(path) {
    const p = path.replaceAll("\\", "/");
    if (!p.endsWith(".md")) return false;
    if (p.startsWith("Docs/QA/") || p.includes("/QA/")) return false;
    if (p.startsWith("Docs/feature-requests/")) return false;
    if (p.startsWith("Docs/recipes/") || p.startsWith("Docs/api-index/")) return true;
    if (p === "README.md" || p === "LICENSE.md") return true;
    if (/testbed/i.test(p) || /^MetalDemo\//i.test(p) || p.startsWith("Samples/")) return false;
    if (p === "Docs/ai-discoverability.md" || p.endsWith("/AI-USAGE.md")) return false;
    if (/^Metal[A-Za-z0-9]+\/README\.md$/.test(p)) return true;
    if (/^Metal[A-Za-z0-9]+\/RossWright\.[^/]+\/README\.md$/.test(p)) return true;
    return false;
  }

  function libraryOf(path) {
    const match = path.match(/^(Metal[A-Za-z0-9]+)\//);
    return match ? match[1] : null;
  }

  function slugify(text) {
    return text
      .trim()
      .toLowerCase()
      .replace(/[^\w\s-]/g, "")
      .replace(/\s+/g, "-");
  }

  function uniqueSlug(text, used) {
    const base = slugify(text) || "section";
    const count = used.get(base) || 0;
    used.set(base, count + 1);
    return count ? `${base}-${count}` : base;
  }

  function normalizeDocPath(path) {
    let clean = decodeURIComponent(path).replace(/^\/+/, "");
    if (clean === "" || clean === "index.html") return "README.md";
    if (clean === "LICENSE") return "LICENSE.md";
    if (!clean.endsWith(".md") && !clean.includes(".")) clean += ".md";
    return clean;
  }

  function parseRoute() {
    const raw = location.hash.replace(/^#\/?/, "");
    const hashIdx = raw.indexOf("#");
    const queryIdx = raw.indexOf("?");
    let filePart = raw;
    let heading = "";
    if (hashIdx >= 0 && (queryIdx < 0 || hashIdx < queryIdx)) {
      heading = decodeURIComponent(raw.slice(hashIdx + 1).split("?")[0]);
      filePart = raw.slice(0, hashIdx);
    } else if (queryIdx >= 0) {
      filePart = raw.slice(0, queryIdx);
      heading = new URLSearchParams(raw.slice(queryIdx + 1)).get("id") || "";
    }
    return { path: normalizeDocPath(filePart), heading };
  }

  function routeHash(path, heading) {
    const hash = heading ? `?id=${encodeURIComponent(heading)}` : "";
    return `#/${path}${hash}`;
  }

  function pageBase() {
    const url = new URL(location.href);
    url.hash = "";
    url.search = "";
    let path = url.pathname;
    if (path.endsWith("index.html")) path = path.slice(0, -"index.html".length);
    else if (!path.endsWith("/")) path = path.slice(0, path.lastIndexOf("/") + 1);
    url.pathname = path || "/";
    return url;
  }

  function docUrl(path) {
    return new URL(path.replace(/^\/+/, ""), pageBase()).href;
  }

  function isFilePreview() {
    return location.protocol === "file:";
  }

  function filePreviewHelp() {
    return `<div class="doc-error">
      <p>Markdown cannot be loaded from a <code>file://</code> URL. Browsers block those fetches.</p>
      <p>From the repository root, serve the site over HTTP:</p>
      <pre><code>python3 -m http.server 8765</code></pre>
      <p>Then open <a href="http://127.0.0.1:8765/">http://127.0.0.1:8765/</a>.</p>
    </div>`;
  }

  async function loadText(path) {
    if (isFilePreview()) throw new Error("file-protocol");
    if (cache.has(path)) return cache.get(path);
    const pending = fetch(docUrl(path)).then(async (res) => {
      if (!res.ok) throw new Error(`${res.status} ${path}`);
      return res.text();
    });
    cache.set(path, pending);
    try {
      return await pending;
    } catch (err) {
      cache.delete(path);
      throw err;
    }
  }

  function extractTitle(markdown, path) {
    const heading = markdown.match(/^#\s+(.+)$/m);
    if (heading) return heading[1].replace(/[`*_]/g, "").trim();
    return fallbackTitle(path);
  }

  function navTitle(path) {
    if (path === "README.md") return "Home";
    if (path === "LICENSE.md") return "License";
    if (path === "Docs/recipes/README.md") return "Recipes";
    if (path === "Docs/api-index/README.md") return "API Index";
    if (path.endsWith("/AI-USAGE.md")) return "AI usage";
    const lib = libraryOf(path);
    if (lib && path === `${lib}/README.md`) return "Overview";
    if (lib && path === `${lib}/RossWright.${lib}/README.md` && !paths.includes(`${lib}/README.md`)) {
      return "Overview";
    }
    if (lib && path.endsWith("/README.md")) {
      return path.split("/").slice(-2, -1)[0].replace(/^RossWright\./, "");
    }
    if (path.startsWith("Docs/api-index/")) {
      return (titles.get(path) || fallbackTitle(path)).replace(/ API Index$/i, "");
    }
    return titles.get(path) || fallbackTitle(path);
  }

  function fallbackTitle(path) {
    if (path === "README.md") return "Overview";
    if (path === "LICENSE.md") return "License";
    if (path.endsWith("/AI-USAGE.md")) return "AI usage";
    const file = path.split("/").pop().replace(/\.md$/, "");
    if (file === "README") {
      const parent = path.split("/").slice(-2, -1)[0];
      return parent.replace(/^RossWright\./, "");
    }
    return file.replace(/-/g, " ");
  }

  function markdownLinks(markdown, fromPath) {
    const found = [];
    const re = /\[[^\]]*]\(([^)]+)\)/g;
    let match;
    while ((match = re.exec(markdown))) {
      const href = match[1].split(/\s+/)[0].replace(/["']/g, "");
      if (!href || href.startsWith("#") || /^(https?:|mailto:)/i.test(href)) continue;
      const resolved = resolveHref(fromPath, href).split("#")[0];
      if (resolved === "LICENSE") found.push("LICENSE.md");
      else if (resolved.endsWith(".md")) found.push(resolved);
    }
    return found;
  }

  function resolveHref(fromPath, href) {
    if (/^(https?:|mailto:)/i.test(href)) return href;
    const base = fromPath.includes("/") ? fromPath.slice(0, fromPath.lastIndexOf("/") + 1) : "";
    const url = new URL(href, `https://rwml.net/${base}`);
    const file = decodeURIComponent(url.pathname.replace(/^\/+/, ""));
    return url.hash ? `${file}${url.hash}` : file;
  }

  async function listFromGithub() {
    try {
      const res = await fetch(TREE, { headers: { Accept: "application/vnd.github+json" } });
      if (!res.ok) return [];
      const data = await res.json();
      return (data.tree || [])
        .filter((item) => item.type === "blob" && isPublicDoc(item.path))
        .map((item) => item.path);
    } catch {
      return [];
    }
  }

  async function crawl(startPaths) {
    const seen = new Set();
    let queue = [...startPaths];
    while (queue.length) {
      const batch = queue.splice(0, 8);
      const discovered = await Promise.all(
        batch.map(async (path) => {
          if (seen.has(path) || !isPublicDoc(path)) return [];
          seen.add(path);
          try {
            const text = await loadText(path);
            titles.set(path, extractTitle(text, path));
            const next = markdownLinks(text, path).filter((linked) => isPublicDoc(linked));
            return next;
          } catch {
            seen.delete(path);
            cache.delete(path);
            return [];
          }
        })
      );
      for (const next of discovered) {
        for (const path of next) {
          if (!seen.has(path)) queue.push(path);
        }
      }
    }
    return [...seen];
  }

  function sortLibraries(names) {
    const known = LIBRARY_ORDER.filter((name) => names.includes(name));
    const extra = names.filter((name) => !LIBRARY_ORDER.includes(name)).sort();
    return [...known, ...extra];
  }

  function groupPaths(all) {
    const groups = {
      libraries: new Map(),
      other: []
    };
    for (const path of all) {
      if (path === "README.md" || path === "LICENSE.md") continue;
      if (path.startsWith("Docs/recipes/") || path.startsWith("Docs/api-index/")) continue;
      if (path.endsWith("/AI-USAGE.md")) continue;
      if (libraryOf(path)) {
        const lib = libraryOf(path);
        if (!groups.libraries.has(lib)) groups.libraries.set(lib, []);
        groups.libraries.get(lib).push(path);
      } else {
        groups.other.push(path);
      }
    }
    const familyFirst = (lib, docs) =>
      docs.slice().sort((a, b) => {
        const score = (path) => {
          if (path === `${lib}/README.md`) return 0;
          if (path === `${lib}/RossWright.${lib}/README.md`) return 1;
          return 2;
        };
        return score(a) - score(b) || navTitle(a).localeCompare(navTitle(b));
      });
    for (const [lib, docs] of groups.libraries) groups.libraries.set(lib, familyFirst(lib, docs));
    return groups;
  }

  function navLink(path, title) {
    const a = document.createElement("a");
    a.href = routeHash(path);
    a.dataset.path = path;
    a.textContent = title || navTitle(path);
    return a;
  }

  function linkIsActive(a, current) {
    const target = a.dataset.path;
    if (target === current) return true;
    if (target === "Docs/recipes/README.md" && current.startsWith("Docs/recipes/")) return true;
    if (target === "Docs/api-index/README.md" && current.startsWith("Docs/api-index/")) return true;
    return false;
  }

  function renderNav() {
    const { path: current } = parseRoute();
    const groups = groupPaths(paths);
    navEl.innerHTML = "";

    const overview = document.createElement("div");
    overview.className = "nav-group";
    overview.innerHTML = `<p class="nav-group-title">Overview</p>`;
    for (const item of [
      ["README.md", "Home"],
      ["Docs/recipes/README.md", "Recipes"],
      ["Docs/api-index/README.md", "API Index"],
      ["LICENSE.md", "License"]
    ]) {
      overview.append(navLink(item[0], item[1]));
    }
    navEl.append(overview);

    const libsWrap = document.createElement("div");
    libsWrap.className = "nav-group";
    libsWrap.innerHTML = `<p class="nav-group-title">Libraries</p>`;
    for (const lib of sortLibraries([...groups.libraries.keys()])) {
      const details = document.createElement("details");
      details.className = "nav-lib";
      details.open = current.startsWith(`${lib}/`);
      const summary = document.createElement("summary");
      summary.textContent = lib;
      details.append(summary);
      for (const path of groups.libraries.get(lib)) details.append(navLink(path));
      libsWrap.append(details);
    }
    navEl.append(libsWrap);

    if (groups.other.length) {
      const other = document.createElement("div");
      other.className = "nav-group";
      other.innerHTML = `<p class="nav-group-title">More</p>`;
      for (const path of groups.other) other.append(navLink(path));
      navEl.append(other);
    }

    navEl.querySelectorAll("a[data-path]").forEach((a) => {
      a.classList.toggle("active", linkIsActive(a, current));
    });
  }

  function rewriteLinks(root, fromPath) {
    root.querySelectorAll("a[href]").forEach((a) => {
      const href = a.getAttribute("href");
      if (!href || /^(mailto:|javascript:)/i.test(href)) return;
      if (href.startsWith("#") && !href.startsWith("#/")) {
        a.setAttribute("href", routeHash(fromPath, href.slice(1)));
        return;
      }
      if (/^https?:/i.test(href)) {
        a.target = "_blank";
        a.rel = "noopener";
        return;
      }
      const [file, hash] = resolveHref(fromPath, href).split("#");
      const docPath = file === "LICENSE" ? "LICENSE.md" : file;
      if (docPath.endsWith(".md") && isPublicDoc(docPath)) {
        a.setAttribute("href", routeHash(docPath, hash));
      } else {
        a.setAttribute("href", BLOB + docPath + (hash ? `#${hash}` : ""));
        a.target = "_blank";
        a.rel = "noopener";
      }
    });
    root.querySelectorAll("img[src]").forEach((img) => {
      const src = img.getAttribute("src");
      if (!src || /^https?:/i.test(src)) return;
      img.setAttribute("src", docUrl(resolveHref(fromPath, src).split("#")[0]));
    });
  }

  function addHeadingIds(root) {
    const used = new Map();
    root.querySelectorAll("h1,h2,h3,h4,h5,h6").forEach((heading) => {
      heading.id = uniqueSlug(heading.textContent, used);
    });
  }

  function renderToc(root, docPath) {
    const headings = [...root.querySelectorAll("h2, h3")];
    tocEl.innerHTML = "";
    headings.forEach((heading) => {
      const a = document.createElement("a");
      a.href = routeHash(docPath, heading.id);
      a.textContent = heading.textContent;
      a.className = `depth-${heading.tagName[1]}`;
      tocEl.append(a);
    });
  }

  function decorateCode(root) {
    root.querySelectorAll("pre code").forEach((block) => {
      const lang = [...block.classList]
        .find((name) => name.startsWith("language-"))
        ?.replace("language-", "")
        .toLowerCase();
      const mapped = LANG[lang] || lang;
      if (mapped && window.hljs?.getLanguage(mapped)) {
        block.classList.add(`language-${mapped}`);
        window.hljs.highlightElement(block);
      }
      const pre = block.parentElement;
      const btn = document.createElement("button");
      btn.type = "button";
      btn.className = "copy-btn";
      btn.textContent = "Copy";
      btn.addEventListener("click", async () => {
        await navigator.clipboard.writeText(block.textContent);
        btn.textContent = "Copied";
        setTimeout(() => (btn.textContent = "Copy"), 1200);
      });
      pre.append(btn);
    });
  }

  function releaseLabel(markdown) {
    const match = markdown.match(/^##\s+New in\s+([0-9.]+)/m);
    return match ? `Current release ${match[1]}` : "";
  }

  function heroHtml(markdown) {
    const release = releaseLabel(markdown);
    return `
      <section class="hero">
        <p class="eyebrow">Pross Co. · .NET 8 / 9 / 10</p>
        <h1>Ross Wright Metal Libraries</h1>
        <p class="lede">Foundations for dependency injection, mediator dispatch, HTTP endpoints, authentication, and console apps — usable on their own or together.</p>
        <div class="hero-actions">
          <a class="btn btn-bronze" href="#/Docs/recipes/README.md">Start with a recipe</a>
          <a class="btn btn-ghost" href="https://www.nuget.org/profiles/RossWright" target="_blank" rel="noopener">Browse NuGet</a>
        </div>
        ${release ? `<p class="release-badge">${release}</p>` : ""}
      </section>`;
  }

  function crumbs(path) {
    const parts = path.replace(/\.md$/, "").split("/");
    return parts.join(" / ");
  }

  let renderedPath = "";

  async function renderPage() {
    const { path: docPath, heading } = parseRoute();
    if (renderedPath === docPath && contentEl.querySelector(".md")) {
      if (heading) {
        requestAnimationFrame(() => document.getElementById(heading)?.scrollIntoView());
      } else window.scrollTo(0, 0);
      renderNav();
      return;
    }
    renderNav();
    contentEl.innerHTML = `<p class="crumb">Loading ${docPath}…</p>`;
    tocEl.innerHTML = "";
    try {
      const markdown = await loadText(docPath);
      titles.set(docPath, extractTitle(markdown, docPath));
      const unsafe = marked.parse(markdown);
      const html = window.DOMPurify.sanitize(unsafe, { USE_PROFILES: { html: true }, ADD_ATTR: ["target", "id"] });
      const isHome = docPath === "README.md";
      contentEl.innerHTML = `${isHome ? heroHtml(markdown) : `<p class="crumb">${crumbs(docPath)}</p>`}<div class="md">${html}</div>`;
      const mdRoot = contentEl.querySelector(".md");
      if (isHome) {
        const firstH1 = mdRoot.querySelector("h1");
        if (firstH1) firstH1.remove();
      }
      addHeadingIds(mdRoot);
      rewriteLinks(mdRoot, docPath);
      decorateCode(mdRoot);
      renderToc(mdRoot, docPath);
      renderNav();
      renderedPath = docPath;
      editEl.hidden = false;
      editEl.href = BLOB + docPath;
      const pageTitle = titles.get(docPath) || "RWML";
      document.title = isHome ? "Ross Wright Metal Libraries" : `${pageTitle} · RWML`;
      closeNav();
      if (heading) {
        requestAnimationFrame(() => document.getElementById(heading)?.scrollIntoView());
      }
    } catch (err) {
      renderedPath = "";
      contentEl.innerHTML = isFilePreview()
        ? filePreviewHelp()
        : `<div class="doc-error"><p>Could not load <code>${docPath}</code>.</p><p>${err.message}</p></div>`;
      editEl.hidden = true;
    }
  }

  function closeNav() {
    document.body.classList.remove("nav-open");
    navToggle.setAttribute("aria-expanded", "false");
    backdrop.hidden = true;
  }

  function openNav() {
    document.body.classList.add("nav-open");
    navToggle.setAttribute("aria-expanded", "true");
    backdrop.hidden = false;
  }

  function openSearch() {
    overlay.hidden = false;
    searchInput.value = "";
    searchResults.innerHTML = "";
    searchInput.focus();
  }

  function closeSearch() {
    overlay.hidden = true;
  }

  function renderSearch(query) {
    const q = query.trim().toLowerCase();
    if (!q) {
      searchResults.innerHTML = "";
      searchHits = [];
      return;
    }
    searchHits = searchIndex
      .map((item) => {
        const titleHit = item.title.toLowerCase().includes(q);
        const pathHit = item.path.toLowerCase().includes(q);
        const idx = item.text.toLowerCase().indexOf(q);
        if (!titleHit && !pathHit && idx < 0) return null;
        const snippet = idx >= 0 ? item.text.slice(Math.max(0, idx - 40), idx + 80).replace(/\s+/g, " ") : item.path;
        const rank = (titleHit ? 3 : 0) + (pathHit ? 2 : 0) + (idx >= 0 ? 1 : 0);
        return { ...item, snippet, rank };
      })
      .filter(Boolean)
      .sort((a, b) => b.rank - a.rank || a.title.localeCompare(b.title))
      .slice(0, 20);
    searchCursor = 0;
    searchResults.innerHTML = searchHits
      .map(
        (hit, i) =>
          `<li><a href="${routeHash(hit.path)}" aria-selected="${i === 0}"><strong>${hit.title}</strong><small>${hit.snippet}</small></a></li>`
      )
      .join("");
  }

  async function buildSearchIndex() {
    searchIndex = [];
    for (const path of paths) {
      try {
        const text = await loadText(path);
        const title = extractTitle(text, path);
        titles.set(path, title);
        searchIndex.push({
          path,
          title,
          text: text.replace(/[#*`[\]()]/g, " ").slice(0, 20000)
        });
      } catch {
        /* skip missing files */
      }
    }
    renderNav();
  }

  navToggle.addEventListener("click", () => {
    document.body.classList.contains("nav-open") ? closeNav() : openNav();
  });
  backdrop.addEventListener("click", closeNav);
  document.querySelector("[data-open-search]").addEventListener("click", openSearch);
  overlay.addEventListener("click", (event) => {
    if (event.target === overlay) closeSearch();
  });
  searchInput.addEventListener("input", () => renderSearch(searchInput.value));
  searchResults.addEventListener("click", (event) => {
    if (event.target.closest("a")) closeSearch();
  });

  document.addEventListener("keydown", (event) => {
    const key = event.key.toLowerCase();
    if ((event.ctrlKey || event.metaKey) && key === "k") {
      event.preventDefault();
      overlay.hidden ? openSearch() : closeSearch();
    } else if (key === "escape") {
      closeSearch();
      closeNav();
    } else if (!overlay.hidden && searchHits.length) {
      if (key === "arrowdown" || key === "arrowup") {
        event.preventDefault();
        searchCursor = (searchCursor + (key === "arrowdown" ? 1 : -1) + searchHits.length) % searchHits.length;
        searchResults.querySelectorAll("a").forEach((a, i) => a.setAttribute("aria-selected", String(i === searchCursor)));
        searchResults.querySelectorAll("a")[searchCursor]?.scrollIntoView({ block: "nearest" });
      } else if (key === "enter") {
        event.preventDefault();
        location.hash = routeHash(searchHits[searchCursor].path).slice(1);
        closeSearch();
      }
    }
  });

  if (/Mac|iPhone|iPad/.test(navigator.userAgent)) {
    const keys = document.querySelectorAll(".search-open kbd");
    if (keys[0]) keys[0].textContent = "⌘";
    keys[1]?.remove();
  }

  window.addEventListener("hashchange", renderPage);

  async function boot() {
    if (isFilePreview()) {
      contentEl.innerHTML = filePreviewHelp();
      return;
    }
    if (!location.hash) history.replaceState(null, "", "#/README.md");
    paths = [...SEED];
    const githubP = listFromGithub();
    const crawledP = crawl(SEED);
    await renderPage();
    const [crawled, githubPaths] = await Promise.all([crawledP, githubP]);
    paths = [...new Set([...SEED, ...crawled, ...githubPaths])].filter(isPublicDoc).sort();
    for (const path of paths) {
      if (!titles.has(path)) titles.set(path, fallbackTitle(path));
    }
    renderNav();
    buildSearchIndex();
  }

  boot();
})();
