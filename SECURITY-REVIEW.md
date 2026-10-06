# Security review — v1.0.1

Reviewed on 2026-10-06. Release commit: `9eb825acb48955632b8f96f7e0f1eb4019a993f3`.

## Outcome

No malicious behavior was found in the reviewed application source. Microsoft Defender's on-demand scans found no threats in the exact published release package or its extracted corresponding source. This is a bounded result from one antivirus engine and a manual source review, not a guarantee of absence of every vulnerability or malware detection.

## Scope and evidence

- Reviewed all 14 application C# files, application XAML, all project/solution files, both test programs, image resources, and both release/build workflows at the release commit.
- Inspected runtime side effects: local settings, Windows API calls, input handling, image loading, startup and shutdown.
- Validated PNG/ICO resources as image files.
- Audited NuGet package advisories using `dotnet list UMADOverlay.sln package --vulnerable --include-transitive`: no vulnerable packages reported. The application has no third-party PackageReference entries. This does not independently audit every component of the bundled Microsoft runtime.
- Downloaded the actual v1.0.1 GitHub release ZIP; its SHA-256 matched the published digest.
- Verified the outer ZIP contains exactly `KefkaSplitOverlay.exe` and `Corresponding-Source.zip`.
- Compared every tracked source file in the included source ZIP to the release commit by SHA-256: matched.
- Microsoft Defender signature update command succeeded and reported no updates needed. Engine: `1.1.26080.3`; antivirus signatures: `1.459.568.0`.
- Release and extracted-source custom scans both completed with exit code 0 and “found no threats.” Defender service and antivirus were enabled; real-time protection on the CI runner was disabled, so these results describe the explicit on-demand scans.
- Authenticode check: `NotSigned`.

[Audit execution and logs](https://github.com/Kinson1012/KefkaSplitOverlay/actions/runs/37431620802) · [Download raw scan evidence](https://github.com/Kinson1012/KefkaSplitOverlay/actions/runs/37431620802/artifacts/11396682740)

### Hashes

Release ZIP SHA-256:

```text
8b80c42304287d4e3c5003905e85a43ca0e4683f900c1acf31194769f96f3ee4
```

EXE SHA-256:

```text
ea49d14cc7ef898818fbba4f140a4b8018b7f991d2ecac5d333c5787e51086d2
```

## Application behavior reviewed

No application code for outbound network requests, credential/browser-token collection, shell or child-process execution, registry/autostart persistence, game-process memory access, global keyboard hooks, remote downloads, or dynamic assembly loading was found.

The settings store reads/writes `%LOCALAPPDATA%/KefkaSplitOverlay/settings.json` and a temporary file beside it. JSON is deserialized into a fixed settings type; language, opacity and window geometry are validated. The application does not request administrative elevation.

The four imported user32.dll functions set this application's window styles and locate monitor bounds. The WPF message hook handles this application's activation and resize messages; it is not a system-wide keyboard hook. Image URIs refer to packaged resources.

## Findings and recommendations

| Finding | Assessment | Recommended change |
| --- | --- | --- |
| Build workflow grants contents: write to the entire job, including checkout/build/tests; checkout persists credentials. | Moderate build-system hardening issue. A compromised action or code executed in a write-authorized run could access repository credentials. Fork PR tokens are normally restricted by GitHub, so this is not a claim that every fork PR receives write access. | Use read-only build jobs, disable checkout credential persistence, and isolate release publishing in a separate job with write permission only there. |
| Workflows use mutable actions/...@v4 tags. | Supply-chain hardening issue; no compromise was observed. | Pin actions to reviewed full commit SHAs and use a controlled update process. |
| EXE lacks an Authenticode publisher signature. | Publisher identity/trust limitation; not evidence of malware. | Sign future releases using a trusted code-signing service/certificate if distribution warrants it. Publish checksums alongside release links. |
| Self-contained EXE bundles its runtime. | Maintenance responsibility rather than an observed exploit. NuGet package audit does not cover all runtime security fixes. | Track Microsoft .NET security updates and rebuild/release when relevant fixes are available. |
| v1.0.0 release workflow can replace that old release asset on rerun. | Release-integrity hardening issue for maintainers. | Retire the one-off old workflow or enforce immutable release assets. |

No high/critical exploitable application vulnerability was identified in this review. Production application code and existing release assets were not changed by this audit.

## Discord warning

The exact Discord warning was not provided, so its cause is unresolved. An unsigned executable and single-file packaging are possible trust/heuristic factors, but the scan does not establish that they caused this warning or that Discord's verdict is a false positive.

.NET single-file native-library extraction is documented behavior of the publish configuration used here:
https://learn.microsoft.com/en-us/dotnet/core/deploying/single-file/overview

If a named antivirus detection appears, retain its exact name and file hash and submit the matching file for vendor investigation:
https://www.microsoft.com/en-us/wdsi/filesubmission

No VirusTotal upload, multi-engine scan, behavioral malware sandbox, or complete binary-to-source reproducibility proof was performed. The EXE's hash identifies the scanned release; source matching verifies the included source, not a reproducible rebuild of the EXE.
