/**
 * Search parameters choosing what an account or fund page shows.
 */
interface DetailViewParams {
  tab?: string;
  startDate?: string;
  endDate?: string;
  startAccountingPeriodId?: string;
  endAccountingPeriodId?: string;
}

const detailViewParamNames = [
  "tab",
  "startDate",
  "endDate",
  "startAccountingPeriodId",
  "endAccountingPeriodId",
] as const;

/**
 * Keeps the selected view and ranges, so saving a change returns to the same view.
 */
const getDetailViewParams = function (
  params: DetailViewParams,
): DetailViewParams {
  return Object.fromEntries(
    detailViewParamNames.flatMap((name) =>
      params[name] === undefined ? [] : [[name, params[name]]],
    ),
  );
};

export type { DetailViewParams };
export default getDetailViewParams;
