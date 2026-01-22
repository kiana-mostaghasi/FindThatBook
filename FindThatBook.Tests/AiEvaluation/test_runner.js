import fs from "fs";

const API_URL = "http://localhost:5063/api/v1/books/search";
const DATASET_FILE = "evaluation_dataset.json";

process.env.NODE_TLS_REJECT_UNAUTHORIZED = "0";

async function runTests() {
  console.log("\nStarting search endpoint evaluation");

  const testCases = JSON.parse(fs.readFileSync(DATASET_FILE, "utf-8"));

  let passed = 0;
  let failed = 0;

  for (const test of testCases) {
    const {
      query,
      expected,
      expected_count,
      type,
      id
    } = test;

    try {
      const response = await fetch(
        `${API_URL}?query=${encodeURIComponent(query)}`
      );
      const json = await response.json();

      if (!json.success) {
        throw new Error(json.message || "API returned success=false");
      }

      const books = json.data ?? [];
      const count = books.length;

      const titles = books.map(b => b.title);
      const titlesLower = titles.map(t => t.toLowerCase());

      const expectedLower = expected?.toLowerCase() ?? "";

      let contentPass = false;
      let countPass = false;

      if (type === "safety") {

        contentPass = count === 0;
      } else if (type === "winner") {

        contentPass =
          count > 0 && titlesLower[0].includes(expectedLower);
      } else {

        contentPass = titlesLower.some(t =>
          t.includes(expectedLower)
        );
      }

      if (expected_count === 0) {
        countPass = count === 0;
      } else if (expected_count === 1) {
        countPass = count === 1;
      } else {

        countPass = count > 1;
      }

      if (contentPass && countPass) {
        console.log(
          `[PASS] (${id}) "${query}" | results=${count}`
        );
        passed++;
      } else {
        console.log(
          `[FAIL] (${id}) "${query}"`
        );

        if (!contentPass) {
          console.log(`  Expected content: "${expected}"`);
        }

        if (!countPass) {
          console.log(
            `  Expected count: ${expected_count}, got: ${count}`
          );
        }

        if (count > 0) {
          console.log(
            `  Top results: ${titles.slice(0, 5).join(" | ")}`
          );
        } else {
          console.log("  Top results: <none>");
        }

        failed++;
      }

    } catch (err) {
      console.log(
        `[ERROR] (${id}) "${query}"`
      );
      console.log(`  ${err.message}`);
      failed++;
    }
  }

  const total = passed + failed;
  const accuracy = total > 0
    ? Math.round((passed / total) * 100)
    : 0;

  console.log(`Total:    ${total}`);
  console.log(`Passed:   ${passed}`);
  console.log(`Failed:   ${failed}`);
  console.log(`Accuracy: ${accuracy}%`);
}

runTests();
