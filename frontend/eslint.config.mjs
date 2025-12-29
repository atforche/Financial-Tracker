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

    extends: [
      eslint.configs.all,
      _import.flatConfigs.recommended,
      _import.flatConfigs.typescript,
      jsdoc.configs["flat/recommended-typescript-error"],
      tseslint.configs.all,
      react.configs.flat.all,
      reactHooks.configs.flat["recommended-latest"],
      reactRefresh.configs.vite,
    ],

    rules: {
      // base eslint rules
      "func-names": "off",
      "id-length": "off",
      "max-lines-per-function": "off",
      "max-statements": "off",
      "new-cap": ["error", { capIsNewExceptions: ["DELETE", "GET", "POST"] }],
      "no-magic-numbers": "off", // handled by typescript-eslint
      "no-ternary": "off",
      "one-var": ["error", "never"],
      "sort-keys": ["off"],

      // import rules
      "import/no-deprecated": "error",
      "import/no-empty-named-blocks": "error",
      "import/no-extraneous-dependencies": "error",
      "import/no-mutable-exports": "error",
      "import/no-named-as-default": "error",
      "import/no-named-as-default-member": "error",
      "import/no-unused-modules": "error",
      "import/no-amd": "error",
      "import/no-commonjs": "error",
      "import/no-import-module-exports": "error",
      "import/no-nodejs-modules": "off",
      "import/unambiguous": "error",
      "import/no-absolute-path": "error",
      "import/no-cycle": "error",
      "import/no-dynamic-require": "error",
      "import/no-relative-packages": "error",
      "import/no-self-import": "error",
      "import/no-useless-path-segments": "error",
      "import/no-webpack-loader-syntax": "error",
      "import/exports-last": "error",
      "import/first": "error",
      "import/group-exports": "error",
      "import/newline-after-import": "error",
      "import/no-anonymous-default-export": "error",
      "import/no-duplicates": "error",
      "import/no-namespace": "error",

      // jsdoc rules
      "jsdoc/check-indentation": "error",
      "jsdoc/check-line-alignment": "error",
      "jsdoc/check-param-names": ["error", { checkDestructured: false }],
      "jsdoc/check-syntax": "error",
      "jsdoc/check-template-names": "error",
      "jsdoc/informative-docs": "error",
      "jsdoc/lines-before-block": "error",
      "jsdoc/no-bad-blocks": "error",
      "jsdoc/no-blank-block-descriptions": "error",
      "jsdoc/no-blank-blocks": "error",
      "jsdoc/require-asterisk-prefix": "error",
      "jsdoc/require-description": "error",
      "jsdoc/require-description-complete-sentence": "error",
      "jsdoc/require-hyphen-before-param-description": "error",
      "jsdoc/require-jsdoc": [
        "error",
        {
          publicOnly: true,
          require: {
            ArrowFunctionExpression: true,
            ClassDeclaration: true,
            ClassExpression: true,
            FunctionDeclaration: true,
            FunctionExpression: true,
            MethodDefinition: true,
          },
          checkGetters: true,
          checkSetters: true,
        },
      ],
      "jsdoc/require-param": ["error", { checkDestructured: false }],
      "jsdoc/ts-method-signature-style": "error",

      // react rules
      "react/forbid-component-props": "off", // temporary
      "react/function-component-definition": [
        "error",
        {
          namedComponents: "function-expression",
          unnamedComponents: "function-expression",
        },
      ],
      "react/jsx-curly-newline": "off",
      "react/jsx-filename-extension": ["error", { extensions: [".tsx"] }],
      "react/jsx-indent": ["error", 2],
      "react/jsx-indent-props": ["error", 2],
      "react/jsx-max-depth": "off",
      "react/jsx-max-props-per-line": "off",
      "react/jsx-newline": "off",
      "react/jsx-no-bind": "off",
      "react/jsx-no-literals": "off",
      "react/jsx-one-expression-per-line": "off",
      "react/jsx-props-no-spreading": "off",
      "react/jsx-sort-props": "off",
      "react/no-multi-comp": "off",
      "react/react-in-jsx-scope": "off",
      "react/require-default-props": "off",

      // react-hooks rules
      "react-hooks/set-state-in-effect": "off",

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
    },
  },
  globalIgnores([
    "**/dist",
    "**/eslint.config.mjs",
    "**/vite-env.d.ts",
    "**/vite.config.ts",
    "**/api.ts",
  ]),
]);
