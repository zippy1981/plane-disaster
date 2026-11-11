---
name: markdown-lint
description: Run this repo's markdown lint locally (markdownlint-cli with .markdownlint.json, the same check as the Lint Markdown workflow) and fix common rule violations. Use when adding or editing any .md file, or when the Lint Markdown check fails on a PR.
---

# Markdown lint

This repo lints every Markdown file with
[markdownlint-cli](https://github.com/igorshubovych/markdownlint-cli). CI runs
it in `.github/workflows/markdown-lint.yml` on pushes and pull requests to
`master` that touch `**/*.md`, `.markdownlint.json`, `.markdownlintignore`, or
the workflow itself.

## Configuration

- Config file: `.markdownlint.json` at the repo root (picked up automatically).
- Ignore file: `.markdownlintignore` (gitignore syntax).
- All default rules are on except `MD013` (line length), which is disabled.
- Do not disable more rules or add inline `markdownlint-disable` comments to
  get a file to pass. Fix the content instead.

## Run it locally

From the repo root, run the same command CI runs:

```sh
npx -y markdownlint-cli --dot '**/*.md'
```

Lint a single file:

```sh
npx -y markdownlint-cli path/to/file.md
```

Let the tool fix what it can, then re-run to see what is left:

```sh
npx -y markdownlint-cli --dot --fix '**/*.md'
```

In PowerShell keep the glob in single quotes so the shell does not expand it.
A clean run prints nothing and exits with code 0.

`--dot` matters: without it the glob skips dot-directories, so files under
`.claude/` and `.github/` would never be linted. `.markdownlintignore` keeps
`node_modules/` and `.claude/worktrees/` out of the run. If a local run still
picks up untracked files, lint only the tracked ones:

```sh
git ls-files '*.md' | xargs npx -y markdownlint-cli
```

## Common violations and fixes

| Rule | Problem | Fix |
| --- | --- | --- |
| MD001 | Heading levels skip (H1 then H3) | Step down one level at a time |
| MD003 | Mixed heading styles | Use ATX headings (`#`, `##`) everywhere |
| MD004 | Mixed list markers | Use `-` for every unordered list |
| MD009 | Trailing spaces | Delete them; do not use two-space line breaks |
| MD010 | Hard tabs | Replace with spaces |
| MD012 | Multiple blank lines | Keep a single blank line |
| MD022 | No blank line around a heading | Add a blank line above and below |
| MD024 | Duplicate heading text | Make each heading unique |
| MD025 | More than one H1 | Keep one H1; demote the others |
| MD026 | Heading ends in punctuation | Remove the trailing `:` or `.` |
| MD029 | Ordered list numbers wrong | Use `1.` `2.` `3.` in order, or all `1.` |
| MD031 | No blank line around a code fence | Add blank lines before and after |
| MD032 | No blank line around a list | Add blank lines before and after |
| MD034 | Bare URL | Wrap it as `<https://...>` or `[text](https://...)` |
| MD040 | Code fence with no language | Add one (`sh`, `powershell`, `dockerfile`, `text`) |
| MD041 | First line is not an H1 | Start the body with `# Title` (front matter is fine) |
| MD047 | File does not end with a newline | End with exactly one newline |

## Front matter

Claude Code agents and skills start with YAML front matter. markdownlint skips
it, and MD041 then expects the first line after the closing `---` to be an
H1. Leave one blank line after the front matter, then the H1.
