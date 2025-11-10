import { defineConfig, globalIgnores } from "eslint/config";
import eslint from "@eslint/js";
import jsdoc from "eslint-plugin-jsdoc";
import _import from "eslint-plugin-import";
import reactRefresh from "eslint-plugin-react-refresh";
import globals from "globals";
import tseslint from "typescript-eslint";

export default defineConfig([
  {
    languageOptions: {
      globals: {
        ...globals.browser,
      },

      parser: tseslint.parser,
      ecmaVersion: 2020,

      parserOptions: {
        allowAutomaticSingleRunInference: true,
        jsDocParsingMode: "all",
        project: ["./tsconfig.json", "./tsconfig.node.json"],
        tsconfigRootDir: import.meta.dirname,
      },
    },

    settings: {
      react: {
        version: "detect",
      },

      "import/resolver": {
        typescript: {},
      },
    },

    plugins: {
      reactRefresh,
      jsdoc,
      import: _import,
    },

    extends: [eslint.configs.all],

    rules: {
      // base eslint rules
      "func-names": "off",
      "id-length": "off",
      "max-lines-per-function": "off",
      "no-magic-numbers": ["error", { ignore: [0, 1] }],
      "no-ternary": "off",
      "no-unused-vars": "off", // temporary failure, handled by typescript-eslint
      "one-var": ["error", "never"],
      "sort-keys": ["off"],
    },
  },
  globalIgnores([
    "**/dist",
    "**/eslint.config.mjs",
    "**/vite-env.d.ts",
    "**/vite.config.ts",
    "**/api.d.ts",
  ]),
]);
