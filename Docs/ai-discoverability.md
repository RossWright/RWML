# AI Discoverability Strategy

The released Metal libraries should be easy for developers and AI coding agents to discover, reference, and use correctly from NuGet packages.

## Layers

1. NuGet metadata: package descriptions, tags, README, license, repository URL, SourceLink, and symbol packages.
2. XML documentation: public consumer APIs have summaries, parameter docs, return docs, and examples where helpful.
3. Package README files: installation, namespaces, common APIs, quick starts, and package selection guidance.
4. AI index files: `llms.txt`, per-library `AI-USAGE.md`, and `Docs/api-index`.
5. Task recipes: `Docs/recipes` pages that map common jobs to packages, namespaces, setup code, and gotchas.
6. Generated API docs: future DocFX or equivalent generated from XML documentation.
7. Samples: future minimal Blazor WebAssembly, ASP.NET Core, console, and shared-contract examples.

## NuGet package inclusion

Every released package should carry its package-family AI card and API index:

- `AI-USAGE.md` at package root.
- `docs/<family>-api.md` under the package `docs` folder.

These files supplement the NuGet README and XML documentation. They are intended for package-cache-aware AI tools and local developer inspection; NuGet.org still displays `README.md` as the primary package page.

## Public API documentation policy

- Document every intended public API in released packages.
- Make implementation details `internal` instead of public where possible.
- If a public API is in an `Internal` folder but must remain public for framework or serializer reasons, document why in XML comments.
- Keep XML summaries concise and action-oriented.
- Mention required setup or namespace when it prevents common mistakes.

## API index maintenance

The files in `Docs/api-index` are hand-maintained scaffolding for now. Update them whenever high-value public APIs are added, renamed, or removed.

## Recipe maintenance

The files in `Docs/recipes` are task-first guides. Add or update a recipe when a released package family gains a new common workflow, setup path, or package-selection decision.

Each recipe should include:

- when to use the package family
- packages to install
- namespaces
- minimal setup code
- one realistic usage snippet
- notes for AI agents and common mistakes

Future automation should:

- Build all released packages.
- Read generated XML documentation.
- Extract public symbols.
- Write package-family API index files.
- Fail CI if package READMEs or API indexes omit newly added intended public APIs.

## Future generated docs site

Use DocFX or an equivalent static documentation generator to publish:

- package overviews
- installation/setup guides
- XML-generated API reference
- API index pages
- Blazor WebAssembly examples
- ASP.NET Core server examples
- shared contracts examples

Recommended output path: `Docs/site` or GitHub Pages.
