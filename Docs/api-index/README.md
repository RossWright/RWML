# Metal Libraries API Index

This directory is a machine-friendly API index for AI agents, documentation generators, and package consumers.

The index is intentionally concise. It complements package READMEs and XML documentation by listing high-value public APIs with package, namespace, signature, and usage guidance.

## Files

| File | Package Family |
|---|---|
| [metalcore-api.md](metalcore-api.md) | MetalCore |
| [metalchain-api.md](metalchain-api.md) | MetalChain |
| [metalcommand-api.md](metalcommand-api.md) | MetalCommand |
| [metalinjection-api.md](metalinjection-api.md) | MetalInjection |
| [metalnexus-api.md](metalnexus-api.md) | MetalNexus |
| [metalguardian-api.md](metalguardian-api.md) | MetalGuardian |

## Maintenance

When a public API is added, renamed, or removed in a released package family, update the matching index file. Prefer exact signatures and include the namespace required by consuming code.

Future automation should generate these files from XML documentation and public symbols during CI.
