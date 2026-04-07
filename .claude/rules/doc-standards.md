# Document Standards

## File Naming
- System docs: `docs/systems/<system-name>.md`
- MCP plans: `docs/mcp/<system-name>-tasks.md`
- Content specs: `docs/content/<category>.md`
- Balance sheets: `docs/balance/<topic>.md`
- Devlogs: `docs/devlog/NNN-<short-title>.md` (3-digit zero-padded)

## Document Structure
Every design document MUST include:
1. **Header**: title, date, author (Claude Code)
2. **Context**: what this document covers and why
3. **Specification**: the actual content
4. **Cross-references**: links to related documents
5. **Open questions**: tagged with `[OPEN]`
6. **Risks**: tagged with `[RISK]`

## Consistency Rules
- Crop names, building names, system names: use exactly identical strings across all documents
- Figures (prices, growth days, thresholds): record a single source in the most specific canonical document
- When a figure appears in multiple documents, all locations other than the canonical document must use `(→ see docs/X.md)` references only, not actual values
- **When recording design figures in architecture documents, a `(→ see canonical)` reference must always be attached; recording figures independently is prohibited**
- **Default values in code examples (defaultValue, tuningParam, etc.) also require a canonical reference comment**: `// → see docs/systems/X.md`
- Within the same document, figures must not be duplicated across sections. Record the figure in one section and reference that section from all others.
- **When extending an enum/type**: All code examples (switch statements, GetXxxMultiplier, etc.) that use that enum must be updated exhaustively within the same document.
- **(PATTERN-005) JSON schema and C# class synchronization**: When a Part I JSON example and a Part II C# class are written together in the same document, all field names and field counts must exist identically on both sides. When adding, removing, or renaming a field, update the JSON example and the C# class in the same edit. The reviewer verifies this through Reviewer Checklist item 9.
- **(PATTERN-006) No array/table figures in MCP task documents**: Do not record array/table-form figures (XP tables, price lists, etc.) directly in MCP task documents (including Part II). Reference canonical documents with `(→ see docs/X.md)` only. When direct recording is unavoidable, add a `// → copied from docs/X.md` comment next to the value and modify the canonical document simultaneously to guarantee identical values.
- **(PATTERN-007) No direct content parameter values in SO asset tables**: Recording content-definition parameters such as `tileSize`, `buildTimeDays`, `recipeCount` as concrete values directly in SO asset data tables (facility asset data tables, etc.) in pipeline/architecture documents such as `data-pipeline.md` is prohibited. Those parameters must reference canonical content documents using the format `(→ see docs/content/facilities.md section X.X)` only. The role of SO asset tables in pipeline documents is limited to defining field types and default value schemas; actual content values are the sole source of truth in content documents.
- **(PATTERN-008) No direct recipe lists in facility documents**: Recording recipe lists (ingredients, outputs, sale prices, processing times, etc.) directly in individual facility sections of facility documents (`facilities.md`, etc.) is prohibited. Recipe information must reference the canonical document (`processing-system.md`) using the format `(→ see docs/content/processing-system.md section X.X)` only. What facility documents are allowed to record is limited to structural parameters of that facility such as recipe slot count, fuel type, and processing speed multiplier.
- **(PATTERN-009) Mandatory history banner for pre-decision analysis sections in balance documents**: In balance documents (gathering-economy.md, crop-economy.md, etc.), analysis sections written before a specific BAL-XXX decision must not be deleted after the decision is confirmed; instead, a `> [History — pre-BAL-XXX analysis]` banner must be added at the top of that section. Having old figures and current figures coexist in the same document without a banner is prohibited. When a decision is confirmed, update the figures in the canonical section and only add a banner to the pre-decision analysis section without deleting it.
- **(PATTERN-010) No placeholder values/IDs in architecture documents**: Recording unconfirmed values or IDs arbitrarily when writing architecture documents in parallel with design documents is prohibited. When values or IDs are needed in an architecture document before design is confirmed, use the explicit placeholder tag `[OPEN - to be filled after DES-XXX is confirmed]`. Synchronize the relevant architecture section immediately after design is confirmed.

## Canonical Data Mapping

When writing a new document, check the mapping below to confirm the canonical source and do not record figures directly.

| Data type | Canonical document | Notes |
|-----------|-------------------|-------|
| Crop names, seed prices, sale prices, growth days | `docs/design.md` section 4.2 | Full crop list |
| Crop growth stages, quality formula, special growth (giant, etc.) | `docs/systems/crop-growth.md` | Growth mechanics overall |
| Tile states, tool interactions, water/fertilizer effects | `docs/systems/farming-system.md` | Cultivation mechanics overall |
| Time slot definitions, weather types, weather probabilities, season transitions | `docs/systems/time-season.md` | Time/weather data |
| Economic figures (starting gold, price bounds, supply/demand adjustments, etc.) | `docs/systems/economy-system.md` | Economy mechanics overall |
| Facility names, construction requirements, upgrade paths | `docs/design.md` section 4.6 | Full facility list |
| BuildingData SO field definitions | `docs/pipeline/data-pipeline.md` Part I section 2.4 | Facility SO schema |
| **Facility tileSize, buildTimeDays, effectRadius and other structural parameters** | **`docs/content/facilities.md`** | **Section 2.1 Construction Process / per-facility detail sections** |
| Unity project folder structure, namespaces | `docs/systems/project-structure.md` | Project structure |
| Per-facility recipe lists (ingredients, outputs, sale prices, processing times) | `docs/content/processing-system.md` | Direct entry in facility documents is prohibited |

## Language
- Document content: Korean (technical terms in English where natural)
- Tags/labels: English (`[OPEN]`, `[RISK]`, `[TODO]`)
- File names: English, kebab-case
