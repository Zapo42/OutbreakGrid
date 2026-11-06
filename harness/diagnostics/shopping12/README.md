# NPC-SHOPPING-12 city validation

This project reuses the read-only observer in ../shopping11/StoreAudit.cs and its unchanged inputs. Always select109 explicitly (the historical observer default stays108). The shared observer now assigns the requested version explicitly, keeping the108 tool valid after the current version advances.

From repository root with .NET8:

```sh
dotnet run --project harness/diagnostics/shopping12/StoreAudit.csproj -c Release -- harness/diagnostics/shopping11/inputs /tmp/shopping109 109
dotnet run --project harness/diagnostics/shopping12/StoreAudit.csproj -c Release -- harness/diagnostics/shopping11/inputs /tmp/shopping108-control 108 delivery
```

Run sequentially. The four unchanged seed214,100x56,five-actor,203meal fixtures run15600ticks each. They are observational trials with known survival/next-day-work failures. Exit0 certifies finite-food accounting, physical delivery-credit checks and exact replay; it does not certify healthy daily routines or native Unity. See ../shopping11/README.md for the observer definitions. Full event arrays/CSV/checkpoints regenerate with this command; compact checked summaries and raw hashes are stored in harness/validation/shopping12. Inputs and their hashes remain in shopping11/inputs and INPUT_HASHES.json.
