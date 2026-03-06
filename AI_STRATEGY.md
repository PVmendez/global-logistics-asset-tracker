# AI Strategy: GitHub Copilot as an Accelerator

## Summary

I drove GLAT with **Spec-Driven Development (SDD)**. The OpenAPI spec was the foundation, but real development was iterative: write specs, generate code, find bugs, adjust, repeat. Copilot accelerated the "write → test → fix" cycle.

---

## The Real Process

### Phase 1: OpenAPI Specification
I wrote endpoints and schemas. Generated the YAML structure with Copilot. Tested in Swagger, found inconsistencies, went back to the specification. After some adjustments it became the contract that served as source of truth.

### Phase 2: Backend
The specification resulted in auto-generated Controllers and DTOs (NSwag). But they needed fixes: missing validations, wrong response headers, broken enums. Established Clean Architecture (4 layers), security decisions. Copilot generated DbContext, Services, auth. Found bugs (missing async/await, broken type conversions), fixed them.

### Phase 3: Frontend
Defined components and UX flow. Copilot generated initial React code. Components didn't connect well to the API, had to refactor prop drilling. TailwindCSS responsive design needed adjustments.

### Phase 4: Infrastructure
Generated docker-compose.yml. Database didn't initialize correctly. Networking between services required adjustments. Had to deal with SQL error 1801, non-persistent volumes, connection timeouts.

---

**It wasn't clean. It was iterative, had bugs, I adjusted and adjusted.**

---

## What Worked

Clear OpenAPI spec as a "contract" prevented major refactoring. Once the contract was solid, Copilot worked well within that framework. Systematic validation after each generation caught problems before production.


**Copilot accelerated the development cycle** Still had to test, adjust, debug. But it happened in hours, not days. The trick: clear specs, validation after every generation, willingness to refactor.

---

## How This Documentation Was Written

This document itself is an example: Copilot drafted it, I reviewed it against reality, corrected inaccuracies, and added specifics. I adjusted the narrative to be honest: messy, iterative, full of debugging and refactoring. Same process as the code.

---

**Methodology**: Spec-Driven Development (SDD) + Iteration + Validation
