# Diff Review Agent

A .NET 8 command-line agent that reviews a unified Git diff and reports actionable possible bugs, risks, missing tests, and style issues. Output is structured JSON by default, or Markdown with `--format markdown`.

## Run the demo

```bash
dotnet run --project ReviewAgent.csproj -- sample.diff
dotnet run --project ReviewAgent.csproj -- --git
dotnet run --project ReviewAgent.csproj -- --git --base main --format markdown
cat sample.diff | dotnet run --project ReviewAgent.csproj -- -
dotnet test tests/ReviewAgent.Tests.csproj
```

By default, the tool uses deterministic offline heuristics, so the demo works without credentials or network access. It checks added lines for likely hard-coded credentials, broad exception handling, long lines, TODO/FIXME markers, and whether source changes include test changes. These checks are deliberately conservative and are not a substitute for a full semantic review.

## How it is built

The agent is implemented in C# with .NET 8. It reads a diff from a file, stdin, or `git diff`, extracts added-line locations, and emits findings with category, severity, file, line, explanation, and suggested action. It also supports an OpenAI-compatible Chat Completions endpoint for model-based review. Set `REVIEW_API_URL` to the full endpoint URL and `REVIEW_MODEL` to the model name; set `REVIEW_API_KEY` only when the endpoint requires it. For example, a local Ollama server can use `http://localhost:11434/v1/chat/completions`. The model is instructed to treat diff contents as untrusted data, return JSON only, and report only supported findings; responses are validated before output. If an explicitly configured model request fails or returns invalid data, the CLI reports an error instead of silently claiming success.

## Limitations and next steps

Diffs are incomplete context: a heuristic can flag a pattern without knowing the surrounding design, and a model can return malformed or speculative feedback. The offline checks are intentionally limited, findings include exact changed-line locations where possible, and model output is schema-validated. Possible next steps include language-aware parsing and test discovery, pull-request providers and inline review comments, and precision/recall evaluation against a curated set of real bugs.
