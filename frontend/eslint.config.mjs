import { defineConfig, globalIgnores } from "eslint/config";
import eslint from "@eslint/js";
import jsdoc from "eslint-plugin-jsdoc";
import _import from "eslint-plugin-import";
import react from "eslint-plugin-react";
import reactHooks from "eslint-plugin-react-hooks";
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

    extends: [
      eslint.configs.all,
      tseslint.configs.all,
      react.configs.flat.all,
      reactHooks.configs.flat["recommended-latest"],
    ],

    rules: {
      // base eslint rules
      "func-names": "off",
      "id-length": "off",
      "max-lines-per-function": "off",
      "new-cap": ["error", { capIsNewExceptions: ["DELETE", "GET", "POST"] }],
      "no-magic-numbers": "off", // handled by typescript-eslint
      "no-ternary": "off",
      "one-var": ["error", "never"],
      "sort-keys": ["off"],

      // typescript-eslint rules
      "@typescript-eslint/no-magic-numbers": ["error", { ignore: [0, 1] }],
      "@typescript-eslint/naming-convention": [
        "error",
        {
          selector: "default",
          format: ["strictCamelCase"],
          leadingUnderscore: "allow",
          trailingUnderscore: "allow",
        },
        {
          selector: "import",
          format: ["strictCamelCase", "StrictPascalCase"],
        },
        {
          selector: "variable",
          format: ["strictCamelCase", "UPPER_CASE", "StrictPascalCase"],
          leadingUnderscore: "allow",
          trailingUnderscore: "allow",
        },
        {
          selector: ["typeLike"],
          format: ["StrictPascalCase"],
        },
        {
          selector: ["enumMember"],
          format: ["StrictPascalCase"],
        },
        {
          selector: "objectLiteralProperty",
          format: null,
        },
        {
          selector: "classProperty",
          modifiers: ["public"],
          format: ["StrictPascalCase"],
        },
      ],
      "@typescript-eslint/prefer-readonly-parameter-types": "off",

      // react rules
      "react/forbid-component-props": "off", // temporary
      "react/function-component-definition": [
        "error",
        {
          namedComponents: "function-expression",
          unnamedComponents: "function-expression",
        },
      ],
      "react/jsx-filename-extension": ["error", { extensions: [".tsx"] }],
      "react/jsx-indent": ["error", 2],
      "react/jsx-indent-props": ["error", 2],
      "react/jsx-max-depth": "off",
      "react/jsx-max-props-per-line": "off",
      "react/jsx-newline": "off",
      "react/jsx-no-literals": "off",
      "react/jsx-one-expression-per-line": "off",
      "react/jsx-sort-props": "off",
      "react/no-multi-comp": "off",
      "react/react-in-jsx-scope": "off",
      "react/require-default-props": "off",
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
