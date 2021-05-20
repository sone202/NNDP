namespace BubbleChartOilWells.Contracts
{
    /// <summary>
    /// Status of the well
    /// </summary>
    public enum OilWellStatus
    {
        /// <summary>
        /// Transit, exploratory, liquidated
        /// </summary>
        TrExpLiq = 1,

        /// <summary>
        /// Transit
        /// </summary>
        Tr = 2,

        /// <summary>
        /// Extracting, pending liquidation
        /// </summary>
        ExtPLiq = 3,

        /// <summary>
        /// Extracting, liquidated
        /// </summary>
        ExtLiq = 4,

        /// <summary>
        /// Transit, liquidated
        /// </summary>
        TrLiq = 5,

        /// <summary>
        /// Injection, inactive this year
        /// </summary>
        InjInactTY = 6,

        /// <summary>
        /// Electrically driven centrifugal pump, pending Liquidation
        /// </summary>
        EDCPPLiq = 7,

        /// <summary>
        /// Bottom hole pump, inactive this year
        /// </summary>
        BHPInactTY = 8,

        /// <summary>
        /// Piezometric
        /// </summary>
        Piez = 9,

        /// <summary>
        /// Electrically driven centrifugal pump, inactive this year
        /// </summary>
        EDCPInactTY = 10,

        /// <summary>
        /// Bottom hole pump
        /// </summary>
        BHP = 11,

        /// <summary>
        /// Bottom hole pump, inactive in previous years
        /// </summary>
        BHPInactPY = 12,

        /// <summary>
        /// Electrically driven centrifugal pump, inactive in previuos year
        /// </summary>
        EDCPInactPY = 13,

        /// <summary>
        /// Bottom hole pump, in accumulation
        /// </summary>
        BHPInAccum = 14,

        /// <summary>
        /// Injection, liquidated
        /// </summary>
        InjLiq = 15,

        /// <summary>
        /// Electrically driven centrifugal pump, in accumulation
        /// </summary>
        EDCPInAccum = 16,

        /// <summary>
        /// Bottom hole pump, in conservation
        /// </summary>
        BHPInCons = 17,

        /// <summary>
        /// Injection, stopped
        /// </summary>
        InjStopped = 18,

        /// <summary>
        /// Injection
        /// </summary>
        Inj = 19,

        /// <summary>
        /// Bottom hole pump, pending liquidation
        /// </summary>
        BHPPLiq = 20,

        /// <summary>
        /// Extracting, in conservation
        /// </summary>
        ExtInCons = 21,

        /// <summary>
        /// Injection, inactive in previous years
        /// </summary>
        InjInactPY = 22,

        /// <summary>
        /// Bottom hole pump, stopped
        /// </summary>
        BHPStopped = 23,

        /// <summary>
        /// Injection, pending liquidation
        /// </summary>
        InjPLiq = 24,

        /// <summary>
        /// Electrically driven centrifugal pump
        /// </summary>
        EDCP = 25
    }
}
