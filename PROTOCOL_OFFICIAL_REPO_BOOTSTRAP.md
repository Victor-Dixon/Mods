# Protocol: Official Repo Bootstrap (≤10 lines)
1) `git init -b main` at repo root
2) Add `.gitignore` (exclude game installs, archives, build outputs)
3) Ensure `README.md` states what is not committed
4) `git add -A && git commit -m "agent-8: repo bootstrap"`
5) `git remote add origin <repo-url>`
6) `git push -u origin main`
7) If push fails: capture exact HTTP status + message in a devlog and post to Discord


