# Apollo repository setup

Target: bb-io/Apollo. Development branch: develop. Production branch: main.

## Initial branch structure

Create the repository with a minimal main baseline (README and .gitignore only), then create develop from that baseline and push the complete app there. This gives the first develop-to-main PR a meaningful diff for engineer review. The initial main baseline must not contain app deployment workflow or app binaries. Only develop receives the app in this task.

## Workflows

.github/workflows/ci.yaml is copied verbatim from bb-io/Contentful's develop branch (retrieved 2026-11-08). It invokes the central Blackbird build/deployment workflow on develop pushes and production deployment on merged PRs targeting main. It contains no credentials, only references to GitHub variables/secrets.

.github/workflows/build.yml builds Release and runs offline tests for pushes/PRs. It does not run live API tests, need Apollo credentials or deploy anything. Deployment invocation remains the existing Blackbird standard; this validation workflow is not a gate on the parallel deployment invocation.

Before the first develop push, verify these are available to the new repository:

- Actions variable GH_BUILD_WORKFLOW
- Actions variable GH_BUILD_REPOSITORY
- Actions secret GH_PAT_TOKEN

An organization administrator may need to add the new repository to selected-repository access for organization secrets/variables and the central workflow token. Existence of the local YAML does not prove repository access or pipeline registration. Copy no secret values from another repository or into files/chat.

## Production review

Keep main's initial baseline minimal. Require an engineer's PR approval through a branch rule/ruleset before merging develop into main, using the organization's existing policy. Enable appropriate status checks after the first validation run registers their names. Do not merge to main as part of initial internal deployment. A production merge triggers deployment to live environments.

## Checks before push

- Use git status and inspect the staged changes; no appsettings.json, bin, obj, test output, debug dumps or ZIP artifacts.
- Retain only appsettings.json.example with blank API key.
- Run local Release build and offline tests.
- Local live checks are user-reported through call search and conversation search. All 52 offline checks passed locally. Conversation detail retrieval was skipped because no accessible candidate was found, so that live path remains pending.
- Replace the placeholder icon before a production release.

## Pending external work

The private repository bb-io/Apollo was created by the user with an initial README on main. Initial source publication targets develop. Deployment variables/secret access and successful pipeline execution must be verified after publication; a source push alone does not prove deployment.
