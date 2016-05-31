angular.module('SweeperUIApp', ['ui.bootstrap'])
    .controller('SweeperUICtrl', function ($scope, $http) {

        window.setTimeout(function () {
            $(".tmp-hide").removeClass("tmp-hide");
        }, 50);

        // Globals
        $scope.pageSize = 11;
        $scope.claims = {};
        $scope.activeTab = 0;
        $scope.today = new Date();

        // Root for all SCB objects
        $scope.SCB = {};

        // SCB objects for all tabs in general
        $scope.SCB.displayState = "main";
        $scope.SCB.claimsCountFull = 0;
        $scope.SCB.claimsTotalFull = 0;
        $scope.SCB.claimsMaximumFull = 0;
        $scope.SCB.claimsAverageFull = 0;
        $scope.SCB.claimsCountFiltered = 0;
        $scope.SCB.claimsTotalFiltered = 0;
        $scope.SCB.claimsMaximumFiltered = 0;
        $scope.SCB.claimsAverageFiltered = 0;
        $scope.SCB.membersCountFull = 0;
        $scope.SCB.membersCountFiltered = 0;
        $scope.SCB.employeesCountFull = 0;
        $scope.SCB.employeesCountFiltered = 0;
        $scope.SCB.nonemployeesCountFull = 0;
        $scope.SCB.nonemployeesCountFiltered = 0;
        $scope.SCB.monthsFiltered = 12;
        $scope.SCB.FullClaimsSummary = [];
        $scope.SCB.FilteredClaimsSummary = [];

        // SCB filtering objects
        $scope.SCB.startDate = "";
        $scope.SCB.startDateAsDate = moment(new Date());
        $scope.SCB.endDate = "";
        $scope.SCB.endDateAsDate = moment(new Date());
        $scope.SCB.lowerBoundICD = "";
        $scope.SCB.upperBoundICD = "";
        $scope.SCB.lowerBoundCPT = "";
        $scope.SCB.upperBoundCPT = "";
        $scope.SCB.lowerBoundClaimAmount = "";
        $scope.SCB.upperBoundClaimAmount = "";
        $scope.SCB.providerNPI = "";
        $scope.SCB.claimantID = "";
        $scope.SCB.claimNumber = "";

        // SCB on-screen filtering objects - pending until "Apply Filter" is clicked
        $scope.SCB.pendingStartDate = "";
        $scope.SCB.pendingEndDate = "";
        $scope.SCB.pendingLowerBoundICD = "";
        $scope.SCB.pendingUpperBoundICD = "";
        $scope.SCB.pendingLowerBoundCPT = "";
        $scope.SCB.pendingUpperBoundCPT = "";
        $scope.SCB.pendingLowerBoundClaimAmount = "";
        $scope.SCB.pendingUpperBoundClaimAmount = "";
        $scope.SCB.pendingProviderNPI = "";
        $scope.SCB.pendingClaimantID = "";
        $scope.SCB.pendingClaimNumber = "";

        // SCB objects for claims tab
        $scope.SCB.ClaimCostsResults = [];
        $scope.SCB.ClaimCosts = {};
        $scope.SCB.ClaimCosts.sort = "StatementDate";
        $scope.SCB.ClaimCosts.sortAsc = false;
        $scope.SCB.ClaimCosts.pageIndex = 0;

        // SCB objects for diagnoses group tab
        $scope.SCB.DiagnosesGroupResults = [];
        $scope.SCB.DiagnosesGroup = {};
        $scope.SCB.DiagnosesGroup.sort = "Group";
        $scope.SCB.DiagnosesGroup.sortAsc = false;
        $scope.SCB.DiagnosesGroup.pageIndex = 0;
        $scope.SCB.DiagnosesGroup.drillShow = true;

        // SCB objects for diagnoses subgroup tab
        $scope.SCB.DiagnosesSubgroupResults = [];
        $scope.SCB.DiagnosesSubgroup = {};
        $scope.SCB.DiagnosesSubgroup.sort = "ICDCode";
        $scope.SCB.DiagnosesSubgroup.sortAsc = false;
        $scope.SCB.DiagnosesSubgroup.pageIndex = 0;
        $scope.SCB.DiagnosesSubgroup.drillShow = false;

        // SCB objects for diagnoses ICD tab
        $scope.SCB.DiagnosesICDResults = [];
        $scope.SCB.DiagnosesICD = {};
        $scope.SCB.DiagnosesICD.sort = "ICDCode";
        $scope.SCB.DiagnosesICD.sortAsc = false;
        $scope.SCB.DiagnosesICD.pageIndex = 0;
        $scope.SCB.DiagnosesICD.drillShow = false;

        // SCB objects for diagnoses ICD claimants tab
        $scope.SCB.DiagnosesICDClaimantsResults = [];
        $scope.SCB.DiagnosesICDClaimants = {};
        $scope.SCB.DiagnosesICDClaimants.sort = "ICDCode";
        $scope.SCB.DiagnosesICDClaimants.sortAsc = false;
        $scope.SCB.DiagnosesICDClaimants.pageIndex = 0;
        $scope.SCB.DiagnosesICDClaimants.drillShow = false;

        // SCB objects for diagnoses ICD events tab
        $scope.SCB.DiagnosesICDEventsResults = [];
        $scope.SCB.DiagnosesICDEvents = {};
        $scope.SCB.DiagnosesICDEvents.sort = "ICDCode";
        $scope.SCB.DiagnosesICDEvents.sortAsc = false;
        $scope.SCB.DiagnosesICDEvents.pageIndex = 0;
        $scope.SCB.DiagnosesICDEvents.drillShow = false;

        // SCB objects for diagnoses ICD claims tab
        $scope.SCB.DiagnosesICDClaimsResults = [];
        $scope.SCB.DiagnosesICDClaims = {};
        $scope.SCB.DiagnosesICDClaims.sort = "ICDCode";
        $scope.SCB.DiagnosesICDClaims.sortAsc = false;
        $scope.SCB.DiagnosesICDClaims.pageIndex = 0;
        $scope.SCB.DiagnosesICDClaims.drillShow = false;

        // SCB objects for providers tab
        $scope.SCB.ProvidersResults = [];
        $scope.SCB.Providers = {};
        $scope.SCB.Providers.sort = "Code";
        $scope.SCB.Providers.sortAsc = false;
        $scope.SCB.Providers.pageIndex = 0;

        // SCB objects for providers claims tab
        $scope.SCB.ProvidersClaimsResults = [];
        $scope.SCB.ProvidersClaims = {};
        $scope.SCB.ProvidersClaims.sort = "Code";
        $scope.SCB.ProvidersClaims.sortAsc = false;
        $scope.SCB.ProvidersClaims.pageIndex = 1;

        // SCB objects for claimants tab
        $scope.SCB.ClaimantsResults = [];
        $scope.SCB.Claimants = {};
        $scope.SCB.Claimants.sort = "FullName";
        $scope.SCB.Claimants.sortAsc = false;
        $scope.SCB.Claimants.pageIndex = 0;

        // SCB objects for procedures code tab
        $scope.SCB.ProceduresResults = [];
        $scope.SCB.Procedures = {};
        $scope.SCB.Procedures.sort = "ProcedureCode";
        $scope.SCB.Procedures.sortAsc = false;
        $scope.SCB.Procedures.pageIndex = 0;
        $scope.SCB.Procedures.drillShow = false;

        // SCB objects for procedures claims tab
        $scope.SCB.ProceduresClaimsResults = [];
        $scope.SCB.ProceduresClaims = {};
        $scope.SCB.ProceduresClaims.sort = "ICDCode";
        $scope.SCB.ProceduresClaims.sortAsc = false;
        $scope.SCB.ProceduresClaims.pageIndex = 0;
        $scope.SCB.ProceduresClaims.drillShow = false;

        // SCB objects for procedures group tab
        $scope.SCB.ProceduresGroupResults = [];
        $scope.SCB.ProceduresGroup = {};
        $scope.SCB.ProceduresGroup.sort = "ProcedureGroup";
        $scope.SCB.ProceduresGroup.sortAsc = false;
        $scope.SCB.ProceduresGroup.pageIndex = 0;
        $scope.SCB.ProceduresGroup.drillShow = true;

        // SCB objects for procedures subgroup tab
        $scope.SCB.ProceduresSubgroupResults = [];
        $scope.SCB.ProceduresSubgroup = {};
        $scope.SCB.ProceduresSubgroup.sort = "ProceduresSubgroup";
        $scope.SCB.ProceduresSubgroup.sortAsc = false;
        $scope.SCB.ProceduresSubgroup.pageIndex = 0;
        $scope.SCB.ProceduresSubgroup.drillShow = false;

        $scope.tabSelectDiagnosesByGroup = function () {
            $scope.activeTab = 0;
            $scope.GetDiagnosesGroupResults();
            $scope.SCB.DiagnosesGroup.drillShow = true;
            $scope.SCB.DiagnosesSubgroup.drillShow = false;
            $scope.SCB.DiagnosesICD.drillShow = false;
            $scope.SCB.DiagnosesICDClaimants.drillShow = false;
            $scope.SCB.DiagnosesICDEvents.drillShow = false;
            $scope.SCB.DiagnosesICDClaims.drillShow = false;
            $scope.GetFilteredClaimsSummary();
        }

        $scope.tabSelectDiagnosesBySubgroup = function () {
            $scope.activeTab = 1;
            $scope.GetDiagnosesSubgroupResults();
            $scope.SCB.DiagnosesGroup.drillShow = false;
            $scope.SCB.DiagnosesSubgroup.drillShow = true;
            $scope.SCB.DiagnosesICD.drillShow = false;
            $scope.SCB.DiagnosesICDClaimants.drillShow = false;
            $scope.SCB.DiagnosesICDEvents.drillShow = false;
            $scope.SCB.DiagnosesICDClaims.drillShow = false;
            $scope.GetFilteredClaimsSummary();
        }

        $scope.tabSelectDiagnosesByICD = function () {
            $scope.activeTab = 2;
            $scope.GetDiagnosesICDResults();
            $scope.SCB.DiagnosesGroup.drillShow = false;
            $scope.SCB.DiagnosesSubgroup.drillShow = false;
            $scope.SCB.DiagnosesICD.drillShow = true;
            $scope.SCB.DiagnosesICDClaimants.drillShow = false;
            $scope.SCB.DiagnosesICDEvents.drillShow = false;
            $scope.SCB.DiagnosesICDClaims.drillShow = false;
            $scope.GetFilteredClaimsSummary();
        }

        $scope.tabSelectDiagnosesByICDClaimants = function () {
            $scope.activeTab = 3;
            $scope.GetDiagnosesICDClaimantsResults();
            $scope.SCB.DiagnosesGroup.drillShow = false;
            $scope.SCB.DiagnosesSubgroup.drillShow = false;
            $scope.SCB.DiagnosesICD.drillShow = false;
            $scope.SCB.DiagnosesICDClaimants.drillShow = true;
            $scope.SCB.DiagnosesICDEvents.drillShow = false;
            $scope.SCB.DiagnosesICDClaims.drillShow = false;
            $scope.GetFilteredClaimsSummary();
        }

        $scope.tabSelectDiagnosesByICDEvents = function () {
            $scope.activeTab = 3;
            $scope.GetDiagnosesICDEventsResults();
            $scope.SCB.DiagnosesGroup.drillShow = false;
            $scope.SCB.DiagnosesSubgroup.drillShow = false;
            $scope.SCB.DiagnosesICD.drillShow = false;
            $scope.SCB.DiagnosesICDClaimants.drillShow = false;
            $scope.SCB.DiagnosesICDEvents.drillShow = true;
            $scope.SCB.DiagnosesICDClaims.drillShow = false;
            $scope.GetFilteredClaimsSummary();
        }

        $scope.tabSelectDiagnosesByICDClaims = function () {
            $scope.activeTab = 3;
            $scope.GetDiagnosesICDClaimsResults();
            $scope.SCB.DiagnosesGroup.drillShow = false;
            $scope.SCB.DiagnosesSubgroup.drillShow = false;
            $scope.SCB.DiagnosesICD.drillShow = false;
            $scope.SCB.DiagnosesICDClaimants.drillShow = false;
            $scope.SCB.DiagnosesICDEvents.drillShow = false;
            $scope.SCB.DiagnosesICDClaims.drillShow = true;
            $scope.GetFilteredClaimsSummary();
        }

        $scope.tabSelectClaimants = function () {
            $scope.activeTab = 0;
            $scope.GetClaimantsResults();
            $scope.GetFilteredClaimsSummary();
        }

        $scope.tabSelectProviders = function () {
            $scope.activeTab = 0;
            $scope.GetProvidersResults();
            $scope.GetFilteredClaimsSummary();
        }

        $scope.tabSelectProvidersClaims = function () {
            $scope.activeTab = 1;
            $scope.GetProvidersClaimsResults();
            $scope.GetFilteredClaimsSummary();
        }

        $scope.tabSelectProcedures = function () {
            $scope.activeTab = 2;
            $scope.GetProceduresCodeResults();
            $scope.GetFilteredClaimsSummary();
        }

        $scope.tabSelectProceduresClaims = function () {
            $scope.activeTab = 3;
            $scope.GetProceduresCodeClaimsResults();
            $scope.GetFilteredClaimsSummary();
        }

        $scope.tabSelectProceduresGroup = function () {
            $scope.activeTab = 0;
            $scope.GetProceduresGroupResults();
            $scope.GetFilteredClaimsSummary();
        }

        $scope.tabSelectProceduresSubgroup = function () {
            $scope.activeTab = 1;
            $scope.GetProceduresSubgroupResults();
            $scope.GetFilteredClaimsSummary();
        }

        $scope.tabSelectClaimCosts = function () {
            $scope.activeTab = 0;
            $scope.GetClaimCostsResults();
            $scope.GetFilteredClaimsSummary();
        }

        $scope.buildClaimCostsPages = function () {
            $scope.sortClaimCosts();

            $scope.SCB.ClaimCosts.pages = [];
            for (var g = 0; g * $scope.pageSize < $scope.SCB.ClaimCostsResults.length; g++) {
                $scope.SCB.ClaimCosts.pages.push(g + 1);
            }

            var startIndex = Math.max(0, Math.min($scope.SCB.ClaimCosts.pageIndex - 4, $scope.SCB.ClaimCosts.pages.length - 9));

            $scope.SCB.ClaimCosts.visiblePages = [];

            for (var g = 0; g < 9 && g + startIndex < $scope.SCB.ClaimCosts.pages.length; g++) {
                $scope.SCB.ClaimCosts.visiblePages.push(g + startIndex + 1);
            }

            $scope.buildClaimCostsVisiblePage($scope.pageSize);
        }

        $scope.buildClaimCostsVisiblePage = function (pageSize) {
            $scope.SCB.ClaimCostsPage = [];
            for (var g = 0; g < pageSize && g + pageSize * $scope.SCB.ClaimCosts.pageIndex < $scope.SCB.ClaimCostsResults.length; g++) {
                $scope.SCB.ClaimCostsPage.push($scope.SCB.ClaimCostsResults[g + pageSize * $scope.SCB.ClaimCosts.pageIndex]);
            }
        }

        $scope.buildDiagnosesGroupPages = function () {
            $scope.sortDiagnosesGroup();

            $scope.SCB.DiagnosesGroup.pages = [];
            for (var g = 0; g * $scope.pageSize < $scope.SCB.DiagnosesGroupResults.length; g++) {
                $scope.SCB.DiagnosesGroup.pages.push(g + 1);
            }

            var startIndex = Math.max(0, Math.min($scope.SCB.DiagnosesGroup.pageIndex - 4, $scope.SCB.DiagnosesGroup.pages.length - 9));

            $scope.SCB.DiagnosesGroup.visiblePages = [];

            for (var g = 0; g < 9 && g + startIndex < $scope.SCB.DiagnosesGroup.pages.length; g++) {
                $scope.SCB.DiagnosesGroup.visiblePages.push(g + startIndex + 1);
            }

            $scope.buildDiagnosesGroupVisiblePage($scope.pageSize);
        }

        $scope.buildDiagnosesSubgroupPages = function () {
            $scope.sortDiagnosesSubgroup();

            $scope.SCB.DiagnosesSubgroup.pages = [];
            for (var g = 0; g * $scope.pageSize < $scope.SCB.DiagnosesSubgroupResults.length; g++) {
                $scope.SCB.DiagnosesSubgroup.pages.push(g + 1);
            }

            var startIndex = Math.max(0, Math.min($scope.SCB.DiagnosesSubgroup.pageIndex - 4, $scope.SCB.DiagnosesSubgroup.pages.length - 9));

            $scope.SCB.DiagnosesSubgroup.visiblePages = [];

            for (var g = 0; g < 9 && g + startIndex < $scope.SCB.DiagnosesSubgroup.pages.length; g++) {
                $scope.SCB.DiagnosesSubgroup.visiblePages.push(g + startIndex + 1);
            }

            $scope.buildDiagnosesSubgroupVisiblePage($scope.pageSize);
        }

        $scope.buildDiagnosesICDPages = function () {
            $scope.sortDiagnosesICD();

            $scope.SCB.DiagnosesICD.pages = [];
            for (var g = 0; g * $scope.pageSize < $scope.SCB.DiagnosesICDResults.length; g++) {
                $scope.SCB.DiagnosesICD.pages.push(g + 1);
            }

            var startIndex = Math.max(0, Math.min($scope.SCB.DiagnosesICD.pageIndex - 4, $scope.SCB.DiagnosesICD.pages.length - 9));

            $scope.SCB.DiagnosesICD.visiblePages = [];

            for (var g = 0; g < 9 && g + startIndex < $scope.SCB.DiagnosesICD.pages.length; g++) {
                $scope.SCB.DiagnosesICD.visiblePages.push(g + startIndex + 1);
            }

            $scope.buildDiagnosesICDVisiblePage($scope.pageSize);
        }

        $scope.buildDiagnosesICDClaimantsPages = function () {
            $scope.sortDiagnosesICDClaimants();

            $scope.SCB.DiagnosesICDClaimants.pages = [];
            for (var g = 0; g * $scope.pageSize < $scope.SCB.DiagnosesICDClaimantsResults.length; g++) {
                $scope.SCB.DiagnosesICDClaimants.pages.push(g + 1);
            }

            var startIndex = Math.max(0, Math.min($scope.SCB.DiagnosesICDClaimants.pageIndex - 4, $scope.SCB.DiagnosesICDClaimants.pages.length - 9));

            $scope.SCB.DiagnosesICDClaimants.visiblePages = [];

            for (var g = 0; g < 9 && g + startIndex < $scope.SCB.DiagnosesICDClaimants.pages.length; g++) {
                $scope.SCB.DiagnosesICDClaimants.visiblePages.push(g + startIndex + 1);
            }

            $scope.buildDiagnosesICDClaimantsVisiblePage($scope.pageSize);
        }

        $scope.buildDiagnosesICDEventsPages = function () {
            $scope.sortDiagnosesICDEvents();

            $scope.SCB.DiagnosesICDEvents.pages = [];
            for (var g = 0; g * $scope.pageSize < $scope.SCB.DiagnosesICDEventsResults.length; g++) {
                $scope.SCB.DiagnosesICDEvents.pages.push(g + 1);
            }

            var startIndex = Math.max(0, Math.min($scope.SCB.DiagnosesICDEvents.pageIndex - 4, $scope.SCB.DiagnosesICDEvents.pages.length - 9));

            $scope.SCB.DiagnosesICDEvents.visiblePages = [];

            for (var g = 0; g < 9 && g + startIndex < $scope.SCB.DiagnosesICDEvents.pages.length; g++) {
                $scope.SCB.DiagnosesICDEvents.visiblePages.push(g + startIndex + 1);
            }

            $scope.buildDiagnosesICDEventsVisiblePage($scope.pageSize);
        }

        $scope.buildDiagnosesICDClaimsPages = function () {
            $scope.sortDiagnosesICDClaims();

            $scope.SCB.DiagnosesICDClaims.pages = [];
            for (var g = 0; g * $scope.pageSize < $scope.SCB.DiagnosesICDClaimsResults.length; g++) {
                $scope.SCB.DiagnosesICDClaims.pages.push(g + 1);
            }

            var startIndex = Math.max(0, Math.min($scope.SCB.DiagnosesICDClaims.pageIndex - 4, $scope.SCB.DiagnosesICDClaims.pages.length - 9));

            $scope.SCB.DiagnosesICDClaims.visiblePages = [];

            for (var g = 0; g < 9 && g + startIndex < $scope.SCB.DiagnosesICDClaims.pages.length; g++) {
                $scope.SCB.DiagnosesICDClaims.visiblePages.push(g + startIndex + 1);
            }

            $scope.buildDiagnosesICDClaimsVisiblePage($scope.pageSize);
        }

        $scope.buildDiagnosesGroupVisiblePage = function (pageSize) {
            $scope.SCB.DiagnosesGroupPage = [];
            for (var g = 0; g < pageSize && g + pageSize * $scope.SCB.DiagnosesGroup.pageIndex < $scope.SCB.DiagnosesGroupResults.length; g++) {
                $scope.SCB.DiagnosesGroupPage.push($scope.SCB.DiagnosesGroupResults[g + pageSize * $scope.SCB.DiagnosesGroup.pageIndex]);
            }
        }

        $scope.buildDiagnosesSubgroupVisiblePage = function (pageSize) {
            $scope.SCB.DiagnosesSubgroupPage = [];
            for (var g = 0; g < pageSize && g + pageSize * $scope.SCB.DiagnosesSubgroup.pageIndex < $scope.SCB.DiagnosesSubgroupResults.length; g++) {
                $scope.SCB.DiagnosesSubgroupPage.push($scope.SCB.DiagnosesSubgroupResults[g + pageSize * $scope.SCB.DiagnosesSubgroup.pageIndex]);
            }
        }

        $scope.buildDiagnosesICDVisiblePage = function (pageSize) {
            $scope.SCB.DiagnosesICDPage = [];
            for (var g = 0; g < pageSize && g + pageSize * $scope.SCB.DiagnosesICD.pageIndex < $scope.SCB.DiagnosesICDResults.length; g++) {
                $scope.SCB.DiagnosesICDPage.push($scope.SCB.DiagnosesICDResults[g + pageSize * $scope.SCB.DiagnosesICD.pageIndex]);
            }
        }

        $scope.buildDiagnosesICDClaimantsVisiblePage = function (pageSize) {
            $scope.SCB.DiagnosesICDClaimantsPage = [];
            for (var g = 0; g < pageSize && g + pageSize * $scope.SCB.DiagnosesICDClaimants.pageIndex < $scope.SCB.DiagnosesICDClaimantsResults.length; g++) {
                $scope.SCB.DiagnosesICDClaimantsPage.push($scope.SCB.DiagnosesICDClaimantsResults[g + pageSize * $scope.SCB.DiagnosesICDClaimants.pageIndex]);
            }
        }

        $scope.buildDiagnosesICDEventsVisiblePage = function (pageSize) {
            $scope.SCB.DiagnosesICDEventsPage = [];
            for (var g = 0; g < pageSize && g + pageSize * $scope.SCB.DiagnosesICDEvents.pageIndex < $scope.SCB.DiagnosesICDEventsResults.length; g++) {
                $scope.SCB.DiagnosesICDEventsPage.push($scope.SCB.DiagnosesICDEventsResults[g + pageSize * $scope.SCB.DiagnosesICDEvents.pageIndex]);
            }
        }

        $scope.buildDiagnosesICDClaimsVisiblePage = function (pageSize) {
            $scope.SCB.DiagnosesICDClaimsPage = [];
            for (var g = 0; g < pageSize && g + pageSize * $scope.SCB.DiagnosesICDClaims.pageIndex < $scope.SCB.DiagnosesICDClaimsResults.length; g++) {
                $scope.SCB.DiagnosesICDClaimsPage.push($scope.SCB.DiagnosesICDClaimsResults[g + pageSize * $scope.SCB.DiagnosesICDClaims.pageIndex]);
            }
        }

        $scope.buildProvidersPages = function () {
            $scope.sortProviders();

            $scope.SCB.Providers.pages = [];
            for (var g = 0; g * $scope.pageSize < $scope.SCB.ProvidersResults.length; g++) {
                $scope.SCB.Providers.pages.push(g + 1);
            }

            var startIndex = Math.max(0, Math.min($scope.SCB.Providers.pageIndex - 4, $scope.SCB.Providers.pages.length - 9));

            $scope.SCB.Providers.visiblePages = [];

            for (var g = 0; g < 9 && g + startIndex < $scope.SCB.Providers.pages.length; g++) {
                $scope.SCB.Providers.visiblePages.push(g + startIndex + 1);
            }

            $scope.buildProvidersVisiblePage($scope.pageSize);
        }

        $scope.buildProvidersVisiblePage = function (pageSize) {
            $scope.SCB.ProvidersPage = [];
            for (var g = 0; g < pageSize && g + pageSize * $scope.SCB.Providers.pageIndex < $scope.SCB.ProvidersResults.length; g++) {
                $scope.SCB.ProvidersPage.push($scope.SCB.ProvidersResults[g + pageSize * $scope.SCB.Providers.pageIndex]);
            }
        }

        $scope.buildProvidersClaimsPages = function () {
            $scope.sortProvidersClaims();

            $scope.SCB.ProvidersClaims.pages = [];
            for (var g = 0; g * $scope.pageSize < $scope.SCB.ProvidersClaimsResults.length; g++) {
                $scope.SCB.ProvidersClaims.pages.push(g + 1);
            }

            var startIndex = Math.max(0, Math.min($scope.SCB.ProvidersClaims.pageIndex - 4, $scope.SCB.ProvidersClaims.pages.length - 9));

            $scope.SCB.ProvidersClaims.visiblePages = [];

            for (var g = 0; g < 9 && g + startIndex < $scope.SCB.ProvidersClaims.pages.length; g++) {
                $scope.SCB.ProvidersClaims.visiblePages.push(g + startIndex + 1);
            }

            $scope.buildProvidersClaimsVisiblePage($scope.pageSize);
        }

        $scope.buildProvidersClaimsVisiblePage = function (pageSize) {
            $scope.SCB.ProvidersClaimsPage = [];
            for (var g = 0; g < pageSize && g + pageSize * $scope.SCB.ProvidersClaims.pageIndex < $scope.SCB.ProvidersClaimsResults.length; g++) {
                $scope.SCB.ProvidersClaimsPage.push($scope.SCB.ProvidersClaimsResults[g + pageSize * $scope.SCB.ProvidersClaims.pageIndex]);
            }
        }

        $scope.buildProceduresPages = function () {
            $scope.sortProcedures();

            $scope.SCB.Procedures.pages = [];
            for (var g = 0; g * $scope.pageSize < $scope.SCB.ProceduresResults.length; g++) {
                $scope.SCB.Procedures.pages.push(g + 1);
            }

            var startIndex = Math.max(0, Math.min($scope.SCB.Procedures.pageIndex - 4, $scope.SCB.Procedures.pages.length - 9));

            $scope.SCB.Procedures.visiblePages = [];

            for (var g = 0; g < 9 && g + startIndex < $scope.SCB.Procedures.pages.length; g++) {
                $scope.SCB.Procedures.visiblePages.push(g + startIndex + 1);
            }

            $scope.buildProceduresVisiblePage($scope.pageSize);
        }

        $scope.buildProceduresVisiblePage = function (pageSize) {
            $scope.SCB.ProceduresPage = [];
            for (var g = 0; g < pageSize && g + pageSize * $scope.SCB.Procedures.pageIndex < $scope.SCB.ProceduresResults.length; g++) {
                $scope.SCB.ProceduresPage.push($scope.SCB.ProceduresResults[g + pageSize * $scope.SCB.Procedures.pageIndex]);
            }
        }

        $scope.buildProceduresClaimsPages = function () {
            $scope.sortProceduresClaims();

            $scope.SCB.ProceduresClaims.pages = [];
            for (var g = 0; g * $scope.pageSize < $scope.SCB.ProceduresClaimsResults.length; g++) {
                $scope.SCB.ProceduresClaims.pages.push(g + 1);
            }

            var startIndex = Math.max(0, Math.min($scope.SCB.ProceduresClaims.pageIndex - 4, $scope.SCB.ProceduresClaims.pages.length - 9));

            $scope.SCB.ProceduresClaims.visiblePages = [];

            for (var g = 0; g < 9 && g + startIndex < $scope.SCB.ProceduresClaims.pages.length; g++) {
                $scope.SCB.ProceduresClaims.visiblePages.push(g + startIndex + 1);
            }

            $scope.buildProceduresClaimsVisiblePage($scope.pageSize);
        }

        $scope.buildProceduresClaimsVisiblePage = function (pageSize) {
            $scope.SCB.ProceduresClaimsPage = [];
            for (var g = 0; g < pageSize && g + pageSize * $scope.SCB.ProceduresClaims.pageIndex < $scope.SCB.ProceduresClaimsResults.length; g++) {
                $scope.SCB.ProceduresClaimsPage.push($scope.SCB.ProceduresClaimsResults[g + pageSize * $scope.SCB.ProceduresClaims.pageIndex]);
            }
        }

        $scope.buildProceduresGroupPages = function () {
            $scope.sortProceduresGroup();

            $scope.SCB.ProceduresGroup.pages = [];
            for (var g = 0; g * $scope.pageSize < $scope.SCB.ProceduresGroupResults.length; g++) {
                $scope.SCB.ProceduresGroup.pages.push(g + 1);
            }

            var startIndex = Math.max(0, Math.min($scope.SCB.ProceduresGroup.pageIndex - 4, $scope.SCB.ProceduresGroup.pages.length - 9));

            $scope.SCB.ProceduresGroup.visiblePages = [];

            for (var g = 0; g < 9 && g + startIndex < $scope.SCB.ProceduresGroup.pages.length; g++) {
                $scope.SCB.ProceduresGroup.visiblePages.push(g + startIndex + 1);
            }

            $scope.buildProceduresGroupVisiblePage($scope.pageSize);
        }

        $scope.buildProceduresGroupVisiblePage = function (pageSize) {
            $scope.SCB.ProceduresGroupPage = [];
            for (var g = 0; g < pageSize && g + pageSize * $scope.SCB.ProceduresGroup.pageIndex < $scope.SCB.ProceduresGroupResults.length; g++) {
                $scope.SCB.ProceduresGroupPage.push($scope.SCB.ProceduresGroupResults[g + pageSize * $scope.SCB.ProceduresGroup.pageIndex]);
            }
        }

        $scope.buildProceduresSubgroupPages = function () {
            $scope.sortProceduresSubgroup();

            $scope.SCB.ProceduresSubgroup.pages = [];
            for (var g = 0; g * $scope.pageSize < $scope.SCB.ProceduresSubgroupResults.length; g++) {
                $scope.SCB.ProceduresSubgroup.pages.push(g + 1);
            }

            var startIndex = Math.max(0, Math.min($scope.SCB.ProceduresSubgroup.pageIndex - 4, $scope.SCB.ProceduresSubgroup.pages.length - 9));

            $scope.SCB.ProceduresSubgroup.visiblePages = [];

            for (var g = 0; g < 9 && g + startIndex < $scope.SCB.ProceduresSubgroup.pages.length; g++) {
                $scope.SCB.ProceduresSubgroup.visiblePages.push(g + startIndex + 1);
            }

            $scope.buildProceduresSubgroupVisiblePage($scope.pageSize);
        }

        $scope.buildProceduresSubgroupVisiblePage = function (pageSize) {
            $scope.SCB.ProceduresSubgroupPage = [];
            for (var g = 0; g < pageSize && g + pageSize * $scope.SCB.ProceduresSubgroup.pageIndex < $scope.SCB.ProceduresSubgroupResults.length; g++) {
                $scope.SCB.ProceduresSubgroupPage.push($scope.SCB.ProceduresSubgroupResults[g + pageSize * $scope.SCB.ProceduresSubgroup.pageIndex]);
            }
        }

        $scope.buildClaimantsPages = function () {
            $scope.sortClaimants();

            $scope.SCB.Claimants.pages = [];
            for (var g = 0; g * $scope.pageSize < $scope.SCB.ClaimantsResults.length; g++) {
                $scope.SCB.Claimants.pages.push(g + 1);
            }

            var startIndex = Math.max(0, Math.min($scope.SCB.Claimants.pageIndex - 4, $scope.SCB.Claimants.pages.length - 9));

            $scope.SCB.Claimants.visiblePages = [];

            for (var g = 0; g < 9 && g + startIndex < $scope.SCB.Claimants.pages.length; g++) {
                $scope.SCB.Claimants.visiblePages.push(g + startIndex + 1);
            }

            $scope.buildClaimantsVisiblePage($scope.pageSize);
        }

        $scope.buildClaimantsVisiblePage = function (pageSize) {
            $scope.SCB.ClaimantsPage = [];
            for (var g = 0; g < pageSize && g + pageSize * $scope.SCB.Claimants.pageIndex < $scope.SCB.ClaimantsResults.length; g++) {
                $scope.SCB.ClaimantsPage.push($scope.SCB.ClaimantsResults[g + pageSize * $scope.SCB.Claimants.pageIndex]);
            }
        }

        $scope.clickClaimCostsPage = function (page) {
            $scope.SCB.ClaimCosts.pageIndex = page;
            $scope.buildClaimCostsPages();
        }

        $scope.clickDiagnosesGroupPage = function (page) {
            $scope.SCB.DiagnosesGroup.pageIndex = page;
            $scope.buildDiagnosesGroupPages();
        }

        $scope.clickDiagnosesSubgroupPage = function (page) {
            $scope.SCB.DiagnosesSubgroup.pageIndex = page;
            $scope.buildDiagnosesSubgroupPages();
        }

        $scope.clickDiagnosesICDPage = function (page) {
            $scope.SCB.DiagnosesICD.pageIndex = page;
            $scope.buildDiagnosesICDPages();
        }

        $scope.clickDiagnosesICDClaimantsPage = function (page) {
            $scope.SCB.DiagnosesICDClaimants.pageIndex = page;
            $scope.buildDiagnosesICDClaimantsPages();
        }

        $scope.clickDiagnosesICDEventsPage = function (page) {
            $scope.SCB.DiagnosesICDEvents.pageIndex = page;
            $scope.buildDiagnosesICDEventsPages();
        }

        $scope.clickDiagnosesICDClaimsPage = function (page) {
            $scope.SCB.DiagnosesICDClaims.pageIndex = page;
            $scope.buildDiagnosesICDClaimsPages();
        }

        $scope.clickProvidersPage = function (page) {
            $scope.SCB.Providers.pageIndex = page;
            $scope.buildProvidersPages();
        }

        $scope.clickProvidersClaimsPage = function (page) {
            $scope.SCB.ProvidersClaims.pageIndex = page;
            $scope.buildProvidersClaimsPages();
        }

        $scope.clickClaimantsPage = function (page) {
            $scope.SCB.Claimants.pageIndex = page;
            $scope.buildClaimantsPages();
        }

        $scope.clickProceduresPage = function (page) {
            $scope.SCB.Procedures.pageIndex = page;
            $scope.buildProceduresPages();
        }

        $scope.clickProceduresClaimsPage = function (page) {
            $scope.SCB.ProceduresClaims.pageIndex = page;
            $scope.buildProceduresClaimsPages();
        }

        $scope.clickProceduresGroupPage = function (page) {
            $scope.SCB.ProceduresGroup.pageIndex = page;
            $scope.buildProceduresGroupPages();
        }

        $scope.clickProceduresSubgroupPage = function (page) {
            $scope.SCB.ProceduresSubgroup.pageIndex = page;
            $scope.buildProceduresSubgroupPages();
        }

        $scope.clickClaimCostsSort = function (sort) {
            if ($scope.SCB.ClaimCosts.sort != sort) {
                $scope.SCB.ClaimCosts.sort = sort;
                $scope.SCB.ClaimCosts.sortAsc = true;
            } else {
                $scope.SCB.ClaimCosts.sortAsc = !$scope.SCB.ClaimCosts.sortAsc;
            }

            $scope.sortClaimCosts();
            $scope.buildClaimCostsPages();
        }

        $scope.clickDiagnosesGroupSort = function (sort) {
            if ($scope.SCB.DiagnosesGroup.sort != sort) {
                $scope.SCB.DiagnosesGroup.sort = sort;
                $scope.SCB.DiagnosesGroup.sortAsc = true;
            } else {
                $scope.SCB.DiagnosesGroup.sortAsc = !$scope.SCB.DiagnosesGroup.sortAsc;
            }

            $scope.sortDiagnosesGroup();
            $scope.buildDiagnosesGroupPages();
        }

        $scope.clickDiagnosesSubgroupSort = function (sort) {
            if ($scope.SCB.DiagnosesSubgroup.sort != sort) {
                $scope.SCB.DiagnosesSubgroup.sort = sort;
                $scope.SCB.DiagnosesSubgroup.sortAsc = true;
            } else {
                $scope.SCB.DiagnosesSubgroup.sortAsc = !$scope.SCB.DiagnosesSubgroup.sortAsc;
            }

            $scope.sortDiagnosesSubgroup();
            $scope.buildDiagnosesSubgroupPages();
        }

        $scope.clickDiagnosesICDSort = function (sort) {
            if ($scope.SCB.DiagnosesICD.sort != sort) {
                $scope.SCB.DiagnosesICD.sort = sort;
                $scope.SCB.DiagnosesICD.sortAsc = true;
            } else {
                $scope.SCB.DiagnosesICD.sortAsc = !$scope.SCB.DiagnosesICD.sortAsc;
            }

            $scope.sortDiagnosesICD();
            $scope.buildDiagnosesICDPages();
        }

        $scope.clickDiagnosesICDClaimantsSort = function (sort) {
            if ($scope.SCB.DiagnosesICDClaimants.sort != sort) {
                $scope.SCB.DiagnosesICDClaimants.sort = sort;
                $scope.SCB.DiagnosesICDClaimants.sortAsc = true;
            } else {
                $scope.SCB.DiagnosesICDClaimants.sortAsc = !$scope.SCB.DiagnosesICDClaimants.sortAsc;
            }

            $scope.sortDiagnosesICDClaimants();
            $scope.buildDiagnosesICDClaimantsPages();
        }

        $scope.clickDiagnosesICDEventsSort = function (sort) {
            if ($scope.SCB.DiagnosesICDEvents.sort != sort) {
                $scope.SCB.DiagnosesICDEvents.sort = sort;
                $scope.SCB.DiagnosesICDEvents.sortAsc = true;
            } else {
                $scope.SCB.DiagnosesICDEvents.sortAsc = !$scope.SCB.DiagnosesICDEvents.sortAsc;
            }

            $scope.sortDiagnosesICDEvents();
            $scope.buildDiagnosesICDEventsPages();
        }

        $scope.clickDiagnosesICDClaimsSort = function (sort) {
            if ($scope.SCB.DiagnosesICDClaims.sort != sort) {
                $scope.SCB.DiagnosesICDClaims.sort = sort;
                $scope.SCB.DiagnosesICDClaims.sortAsc = true;
            } else {
                $scope.SCB.DiagnosesICDClaims.sortAsc = !$scope.SCB.DiagnosesICDClaims.sortAsc;
            }

            $scope.sortDiagnosesICDClaims();
            $scope.buildDiagnosesICDClaimsPages();
        }

        $scope.clickProvidersSort = function (sort) {
            if ($scope.SCB.Providers.sort != sort) {
                $scope.SCB.Providers.sort = sort;
                $scope.SCB.Providers.sortAsc = true;
            } else {
                $scope.SCB.Providers.sortAsc = !$scope.SCB.Providers.sortAsc;
            }

            $scope.sortProviders();
            $scope.buildProvidersPages();
        }

        $scope.clickProvidersClaimsSort = function (sort) {
            if ($scope.SCB.ProvidersClaims.sort != sort) {
                $scope.SCB.ProvidersClaims.sort = sort;
                $scope.SCB.ProvidersClaims.sortAsc = true;
            } else {
                $scope.SCB.ProvidersClaims.sortAsc = !$scope.SCB.ProvidersClaims.sortAsc;
            }

            $scope.sortProvidersClaims();
            $scope.buildProvidersClaimsPages();
        }

        $scope.clickProceduresSort = function (sort) {
            if ($scope.SCB.Procedures.sort != sort) {
                $scope.SCB.Procedures.sort = sort;
                $scope.SCB.Procedures.sortAsc = true;
            } else {
                $scope.SCB.Procedures.sortAsc = !$scope.SCB.Procedures.sortAsc;
            }

            $scope.sortProcedures();
            $scope.buildProceduresPages();
        }

        $scope.clickProceduresClaimsSort = function (sort) {
            if ($scope.SCB.ProceduresClaims.sort != sort) {
                $scope.SCB.ProceduresClaims.sort = sort;
                $scope.SCB.ProceduresClaims.sortAsc = true;
            } else {
                $scope.SCB.ProceduresClaims.sortAsc = !$scope.SCB.ProceduresClaims.sortAsc;
            }

            $scope.sortProceduresClaims();
            $scope.buildProceduresClaimsPages();
        }

        $scope.clickProceduresGroupSort = function (sort) {
            if ($scope.SCB.ProceduresGroup.sort != sort) {
                $scope.SCB.ProceduresGroup.sort = sort;
                $scope.SCB.ProceduresGroup.sortAsc = true;
            } else {
                $scope.SCB.ProceduresGroup.sortAsc = !$scope.SCB.ProceduresGroup.sortAsc;
            }

            $scope.sortProceduresGroup();
            $scope.buildProceduresGroupPages();
        }

        $scope.clickProceduresSubgroupSort = function (sort) {
            if ($scope.SCB.ProceduresSubgroup.sort != sort) {
                $scope.SCB.ProceduresSubgroup.sort = sort;
                $scope.SCB.ProceduresSubgroup.sortAsc = true;
            } else {
                $scope.SCB.ProceduresSubgroup.sortAsc = !$scope.SCB.ProceduresSubgroup.sortAsc;
            }

            $scope.sortProceduresSubgroup();
            $scope.buildProceduresSubgroupPages();
        }

        $scope.clickClaimantsSort = function (sort) {
            if ($scope.SCB.Claimants.sort != sort) {
                $scope.SCB.Claimants.sort = sort;
                $scope.SCB.Claimants.sortAsc = true;
            } else {
                $scope.SCB.Claimants.sortAsc = !$scope.SCB.Claimants.sortAsc;
            }

            $scope.sortClaimants();
            $scope.buildClaimantsPages();
        }

        $scope.sortClaimCosts = function () {
            var sort = $scope.SCB.ClaimCosts.sort;
            var sorted = [];
            var tmp = $scope.SCB.ClaimCostsResults.concat([]);

            while (tmp.length > 0) {
                var low = 0;

                for (var g = 0; g < tmp.length; g++) {
                    if (sort == "DateOfBirth" || sort == "StatementDate") {
                        if ($scope.SCB.ClaimCosts.sortAsc) {
                            if (new Date(tmp[g][sort]) < new Date(tmp[low][sort])) {
                                low = g;
                            }
                        } else {
                            if (new Date(tmp[g][sort]) > new Date(tmp[low][sort])) {
                                low = g;
                            }
                        }
                    } else {
                        if ($scope.SCB.ClaimCosts.sortAsc) {
                            if (tmp[g][sort] < tmp[low][sort]) {
                                low = g;
                            }
                        } else {
                            if (tmp[g][sort] > tmp[low][sort]) {
                                low = g;
                            }
                        }
                    }
                }

                sorted.push(tmp[low]);
                tmp.splice(low, 1);
            }

            $scope.SCB.ClaimCostsResults = sorted;
        }

        $scope.sortDiagnosesGroup = function () {
            var sort = $scope.SCB.DiagnosesGroup.sort;
            var sorted = [];
            var tmp = $scope.SCB.DiagnosesGroupResults.concat([]);

            while (tmp.length > 0) {
                var low = 0;

                for (var g = 0; g < tmp.length; g++) {
                    if (sort == "DateOfBirth" || sort == "StatementDate") {
                        if ($scope.SCB.DiagnosesGroup.sortAsc) {
                            if (tmp[g][sort] < tmp[low][sort]) {
                                low = g;
                            }
                        } else {
                            if (tmp[g][sort] > tmp[low][sort]) {
                                low = g;
                            }
                        }
                    } else {
                        if ($scope.SCB.DiagnosesGroup.sortAsc) {
                            if (tmp[g][sort] < tmp[low][sort]) {
                                low = g;
                            }
                        } else {
                            if (tmp[g][sort] > tmp[low][sort]) {
                                low = g;
                            }
                        }
                    }
                }

                sorted.push(tmp[low]);
                tmp.splice(low, 1);
            }

            $scope.SCB.DiagnosesGroupResults = sorted;
        }

        $scope.sortDiagnosesSubgroup = function () {
            var sort = $scope.SCB.DiagnosesSubgroup.sort;
            var sorted = [];
            var tmp = $scope.SCB.DiagnosesSubgroupResults.concat([]);

            while (tmp.length > 0) {
                var low = 0;

                for (var g = 0; g < tmp.length; g++) {
                    if (sort == "DateOfBirth" || sort == "StatementDate") {
                        if ($scope.SCB.DiagnosesSubgroup.sortAsc) {
                            if (tmp[g][sort] < tmp[low][sort]) {
                                low = g;
                            }
                        } else {
                            if (tmp[g][sort] > tmp[low][sort]) {
                                low = g;
                            }
                        }
                    } else {
                        if ($scope.SCB.DiagnosesSubgroup.sortAsc) {
                            if (tmp[g][sort] < tmp[low][sort]) {
                                low = g;
                            }
                        } else {
                            if (tmp[g][sort] > tmp[low][sort]) {
                                low = g;
                            }
                        }
                    }
                }

                sorted.push(tmp[low]);
                tmp.splice(low, 1);
            }

            $scope.SCB.DiagnosesSubgroupResults = sorted;
        }

        $scope.sortDiagnosesICD = function () {
            var sort = $scope.SCB.DiagnosesICD.sort;
            var sorted = [];
            var tmp = $scope.SCB.DiagnosesICDResults.concat([]);

            while (tmp.length > 0) {
                var low = 0;

                for (var g = 0; g < tmp.length; g++) {
                    if (sort == "DateOfBirth" || sort == "StatementDate") {
                        if ($scope.SCB.DiagnosesICD.sortAsc) {
                            if (tmp[g][sort] < tmp[low][sort]) {
                                low = g;
                            }
                        } else {
                            if (tmp[g][sort] > tmp[low][sort]) {
                                low = g;
                            }
                        }
                    } else {
                        if ($scope.SCB.DiagnosesICD.sortAsc) {
                            if (tmp[g][sort] < tmp[low][sort]) {
                                low = g;
                            }
                        } else {
                            if (tmp[g][sort] > tmp[low][sort]) {
                                low = g;
                            }
                        }
                    }
                }

                sorted.push(tmp[low]);
                tmp.splice(low, 1);
            }

            $scope.SCB.DiagnosesICDResults = sorted;
        }

        $scope.sortDiagnosesICDClaimants = function () {
            var sort = $scope.SCB.DiagnosesICDClaimants.sort;
            var sorted = [];
            var tmp = $scope.SCB.DiagnosesICDClaimantsResults.concat([]);

            while (tmp.length > 0) {
                var low = 0;

                for (var g = 0; g < tmp.length; g++) {
                    if (sort == "DateOfBirth" || sort == "StatementDate") {
                        if ($scope.SCB.DiagnosesICDClaimants.sortAsc) {
                            if (tmp[g][sort] < tmp[low][sort]) {
                                low = g;
                            }
                        } else {
                            if (tmp[g][sort] > tmp[low][sort]) {
                                low = g;
                            }
                        }
                    } else {
                        if ($scope.SCB.DiagnosesICDClaimants.sortAsc) {
                            if (tmp[g][sort] < tmp[low][sort]) {
                                low = g;
                            }
                        } else {
                            if (tmp[g][sort] > tmp[low][sort]) {
                                low = g;
                            }
                        }
                    }
                }

                sorted.push(tmp[low]);
                tmp.splice(low, 1);
            }

            $scope.SCB.DiagnosesICDClaimantsResults = sorted;
        }

        $scope.sortDiagnosesICDEvents = function () {
            var sort = $scope.SCB.DiagnosesICDEvents.sort;
            var sorted = [];
            var tmp = $scope.SCB.DiagnosesICDEventsResults.concat([]);

            while (tmp.length > 0) {
                var low = 0;

                for (var g = 0; g < tmp.length; g++) {
                    if (sort == "DateOfBirth" || sort == "StatementDate") {
                        if ($scope.SCB.DiagnosesICDEvents.sortAsc) {
                            if (tmp[g][sort] < tmp[low][sort]) {
                                low = g;
                            }
                        } else {
                            if (tmp[g][sort] > tmp[low][sort]) {
                                low = g;
                            }
                        }
                    } else {
                        if ($scope.SCB.DiagnosesICDEvents.sortAsc) {
                            if (tmp[g][sort] < tmp[low][sort]) {
                                low = g;
                            }
                        } else {
                            if (tmp[g][sort] > tmp[low][sort]) {
                                low = g;
                            }
                        }
                    }
                }

                sorted.push(tmp[low]);
                tmp.splice(low, 1);
            }

            $scope.SCB.DiagnosesICDEventsResults = sorted;
        }

        $scope.sortDiagnosesICDClaims = function () {
            var sort = $scope.SCB.DiagnosesICDClaims.sort;
            var sorted = [];
            var tmp = $scope.SCB.DiagnosesICDClaimsResults.concat([]);

            while (tmp.length > 0) {
                var low = 0;

                for (var g = 0; g < tmp.length; g++) {
                    if (sort == "DateOfBirth" || sort == "StatementDate") {
                        if ($scope.SCB.DiagnosesICDClaims.sortAsc) {
                            if (tmp[g][sort] < tmp[low][sort]) {
                                low = g;
                            }
                        } else {
                            if (tmp[g][sort] > tmp[low][sort]) {
                                low = g;
                            }
                        }
                    } else {
                        if ($scope.SCB.DiagnosesICDClaims.sortAsc) {
                            if (tmp[g][sort] < tmp[low][sort]) {
                                low = g;
                            }
                        } else {
                            if (tmp[g][sort] > tmp[low][sort]) {
                                low = g;
                            }
                        }
                    }
                }

                sorted.push(tmp[low]);
                tmp.splice(low, 1);
            }

            $scope.SCB.DiagnosesICDClaimsResults = sorted;
        }

        $scope.sortProviders = function () {
            var sort = $scope.SCB.Providers.sort;
            var sorted = [];
            var tmp = $scope.SCB.ProvidersResults.concat([]);

            while (tmp.length > 0) {
                var low = 0;

                for (var g = 0; g < tmp.length; g++) {
                    if (sort == "DateOfBirth") {
                        if ($scope.SCB.Providers.sortAsc) {
                            if (tmp[g][sort] < tmp[low][sort]) {
                                low = g;
                            }
                        } else {
                            if (tmp[g][sort] > tmp[low][sort]) {
                                low = g;
                            }
                        }
                    } else {
                        if ($scope.SCB.Providers.sortAsc) {
                            if (tmp[g][sort] < tmp[low][sort]) {
                                low = g;
                            }
                        } else {
                            if (tmp[g][sort] > tmp[low][sort]) {
                                low = g;
                            }
                        }
                    }
                }

                sorted.push(tmp[low]);
                tmp.splice(low, 1);
            }

            $scope.SCB.ProvidersResults = sorted;
        }

        $scope.sortProvidersClaims = function () {
            var sort = $scope.SCB.ProvidersClaims.sort;
            var sorted = [];
            var tmp = $scope.SCB.ProvidersClaimsResults.concat([]);

            while (tmp.length > 0) {
                var low = 0;

                for (var g = 0; g < tmp.length; g++) {
                    if (sort == "DateOfBirth") {
                        if ($scope.SCB.ProvidersClaims.sortAsc) {
                            if (tmp[g][sort] < tmp[low][sort]) {
                                low = g;
                            }
                        } else {
                            if (tmp[g][sort] > tmp[low][sort]) {
                                low = g;
                            }
                        }
                    } else {
                        if ($scope.SCB.ProvidersClaims.sortAsc) {
                            if (tmp[g][sort] < tmp[low][sort]) {
                                low = g;
                            }
                        } else {
                            if (tmp[g][sort] > tmp[low][sort]) {
                                low = g;
                            }
                        }
                    }
                }

                sorted.push(tmp[low]);
                tmp.splice(low, 1);
            }

            $scope.SCB.ProvidersClaimsResults = sorted;
        }

        $scope.sortProcedures = function () {
            var sort = $scope.SCB.Procedures.sort;
            var sorted = [];
            var tmp = $scope.SCB.ProceduresResults.concat([]);

            while (tmp.length > 0) {
                var low = 0;

                for (var g = 0; g < tmp.length; g++) {
                    if (sort == "DateOfBirth") {
                        if ($scope.SCB.Procedures.sortAsc) {
                            if (tmp[g][sort] < tmp[low][sort]) {
                                low = g;
                            }
                        } else {
                            if (tmp[g][sort] > tmp[low][sort]) {
                                low = g;
                            }
                        }
                    } else {
                        if ($scope.SCB.Procedures.sortAsc) {
                            if (tmp[g][sort] < tmp[low][sort]) {
                                low = g;
                            }
                        } else {
                            if (tmp[g][sort] > tmp[low][sort]) {
                                low = g;
                            }
                        }
                    }
                }

                sorted.push(tmp[low]);
                tmp.splice(low, 1);
            }

            $scope.SCB.ProceduresResults = sorted;
        }

        $scope.sortProceduresClaims = function () {
            var sort = $scope.SCB.ProceduresClaims.sort;
            var sorted = [];
            var tmp = $scope.SCB.ProceduresClaimsResults.concat([]);

            while (tmp.length > 0) {
                var low = 0;

                for (var g = 0; g < tmp.length; g++) {
                    if (sort == "DateOfBirth") {
                        if ($scope.SCB.ProceduresClaims.sortAsc) {
                            if (tmp[g][sort] < tmp[low][sort]) {
                                low = g;
                            }
                        } else {
                            if (tmp[g][sort] > tmp[low][sort]) {
                                low = g;
                            }
                        }
                    } else {
                        if ($scope.SCB.ProceduresClaims.sortAsc) {
                            if (tmp[g][sort] < tmp[low][sort]) {
                                low = g;
                            }
                        } else {
                            if (tmp[g][sort] > tmp[low][sort]) {
                                low = g;
                            }
                        }
                    }
                }

                sorted.push(tmp[low]);
                tmp.splice(low, 1);
            }

            $scope.SCB.ProceduresClaimsResults = sorted;
        }

        $scope.sortProceduresGroup = function () {
            var sort = $scope.SCB.ProceduresGroup.sort;
            var sorted = [];
            var tmp = $scope.SCB.ProceduresGroupResults.concat([]);

            while (tmp.length > 0) {
                var low = 0;

                for (var g = 0; g < tmp.length; g++) {
                    if (sort == "DateOfBirth") {
                        if ($scope.SCB.ProceduresGroup.sortAsc) {
                            if (tmp[g][sort] < tmp[low][sort]) {
                                low = g;
                            }
                        } else {
                            if (tmp[g][sort] > tmp[low][sort]) {
                                low = g;
                            }
                        }
                    } else {
                        if ($scope.SCB.ProceduresGroup.sortAsc) {
                            if (tmp[g][sort] < tmp[low][sort]) {
                                low = g;
                            }
                        } else {
                            if (tmp[g][sort] > tmp[low][sort]) {
                                low = g;
                            }
                        }
                    }
                }

                sorted.push(tmp[low]);
                tmp.splice(low, 1);
            }

            $scope.SCB.ProceduresGroupResults = sorted;
        }

        $scope.sortProceduresSubgroup = function () {
            var sort = $scope.SCB.ProceduresSubgroup.sort;
            var sorted = [];
            var tmp = $scope.SCB.ProceduresSubgroupResults.concat([]);

            while (tmp.length > 0) {
                var low = 0;

                for (var g = 0; g < tmp.length; g++) {
                    if (sort == "DateOfBirth") {
                        if ($scope.SCB.ProceduresSubgroup.sortAsc) {
                            if (tmp[g][sort] < tmp[low][sort]) {
                                low = g;
                            }
                        } else {
                            if (tmp[g][sort] > tmp[low][sort]) {
                                low = g;
                            }
                        }
                    } else {
                        if ($scope.SCB.ProceduresSubgroup.sortAsc) {
                            if (tmp[g][sort] < tmp[low][sort]) {
                                low = g;
                            }
                        } else {
                            if (tmp[g][sort] > tmp[low][sort]) {
                                low = g;
                            }
                        }
                    }
                }

                sorted.push(tmp[low]);
                tmp.splice(low, 1);
            }

            $scope.SCB.ProceduresSubgroupResults = sorted;
        }

        $scope.sortClaimants = function () {
            var sort = $scope.SCB.Claimants.sort;
            var sorted = [];
            var tmp = $scope.SCB.ClaimantsResults.concat([]);

            while (tmp.length > 0) {
                var low = 0;

                for (var g = 0; g < tmp.length; g++) {
                    if (sort == "DateOfBirth") {
                        if ($scope.SCB.Claimants.sortAsc) {
                            if (tmp[g][sort] < tmp[low][sort]) {
                                low = g;
                            }
                        } else {
                            if (tmp[g][sort] > tmp[low][sort]) {
                                low = g;
                            }
                        }
                    } else {
                        if ($scope.SCB.Claimants.sortAsc) {
                            if (tmp[g][sort] < tmp[low][sort]) {
                                low = g;
                            }
                        } else {
                            if (tmp[g][sort] > tmp[low][sort]) {
                                low = g;
                            }
                        }
                    }
                }

                sorted.push(tmp[low]);
                tmp.splice(low, 1);
            }

            $scope.SCB.ClaimantsResults = sorted;
        }

        // Filtering Options Utilities
        $scope.openFilteringOptions = function () {
            $scope.SCB.displayState = "filters";
            $scope.SCB.pendingStartDate = $scope.SCB.startDate;
            $scope.SCB.pendingEndDate = $scope.SCB.endDate;
            $scope.SCB.pendingLowerBoundClaimAmount = $scope.SCB.lowerBoundClaimAmount;
            $scope.SCB.pendingUpperBoundClaimAmount = $scope.SCB.upperBoundClaimAmount;
            $scope.SCB.pendingLowerBoundICD = $scope.SCB.lowerBoundICD;
            $scope.SCB.pendingUpperBoundICD = $scope.SCB.upperBoundICD;
            $scope.SCB.pendingLowerBoundCPT = $scope.SCB.lowerBoundCPT;
            $scope.SCB.pendingUpperBoundCPT = $scope.SCB.upperBoundCPT;
            $scope.SCB.pendingClaimNumber = $scope.SCB.claimNumber;
            $scope.SCB.pendingClaimantID = $scope.SCB.claimantID;
        }

        $scope.applyFilteringOptions = function () {
            $scope.SCB.displayState = "main";
            $scope.SCB.startDate = $scope.SCB.pendingStartDate;
            $scope.SCB.endDate = $scope.SCB.pendingEndDate;
            if ($scope.SCB.startDate.length == 0 && $scope.SCB.endDate.length == 0)
            {
                $scope.SCB.startDateAsDate = moment(new Date()).subtract(1, "year");
                $scope.SCB.endDateAsDate = moment(new Date());
            }
            else if ($scope.SCB.startDate.length > 0 && $scope.SCB.endDate.length > 0) {
                $scope.SCB.startDateAsDate = moment($scope.SCB.startDate);
                $scope.SCB.endDateAsDate = moment($scope.SCB.endDate);
            }
            else if ($scope.SCB.startDate.length > 0) {
                $scope.SCB.startDateAsDate = moment($scope.SCB.startDate);
                $scope.SCB.endDateAsDate = moment(new Date());
            }
            else
            {
                $scope.SCB.endDateAsDate = moment($scope.SCB.endDate);
                $scope.SCB.startDateAsDate = moment($scope.SCB.endDateAsDate).subtract(1, "year");
            }
            $scope.SCB.monthsFiltered = moment.duration($scope.SCB.endDateAsDate.diff($scope.SCB.startDateAsDate)).asMonths();
            $scope.SCB.lowerBoundClaimAmount = $scope.SCB.pendingLowerBoundClaimAmount;
            $scope.SCB.upperBoundClaimAmount = $scope.SCB.pendingUpperBoundClaimAmount;
            $scope.SCB.lowerBoundICD = $scope.SCB.pendingLowerBoundICD;
            $scope.SCB.upperBoundICD = $scope.SCB.pendingUpperBoundICD;
            $scope.SCB.lowerBoundCPT = $scope.SCB.pendingLowerBoundCPT;
            $scope.SCB.upperBoundCPT = $scope.SCB.pendingUpperBoundCPT;
            $scope.SCB.claimNumber = $scope.SCB.pendingClaimNumber;
            $scope.SCB.claimantID = $scope.SCB.pendingClaimantID;
        }

        $scope.clearStartDate = function () {
            $scope.SCB.pendingStartDate = "";
        }

        $scope.clearEndDate = function () {
            $scope.SCB.pendingEndDate = "";
        }

        $scope.clearLowerBoundClaimAmount = function () {
            $scope.SCB.pendingLowerBoundClaimAmount = "";
        }

        $scope.clearUpperBoundClaimAmount = function () {
            $scope.SCB.pendingUpperBoundClaimAmount = "";
        }

        $scope.clearLowerBoundICD = function () {
            $scope.SCB.pendingLowerBoundICD = "";
        }

        $scope.clearUpperBoundICD = function () {
            $scope.SCB.pendingUpperBoundICD = "";
        }

        $scope.clearLowerBoundCPT = function () {
            $scope.SCB.pendingLowerBoundCPT = "";
        }

        $scope.clearUpperBoundCPT = function () {
            $scope.SCB.pendingUpperBoundCPT = "";
        }

        $scope.clearClaimNumber = function () {
            $scope.SCB.pendingClaimNumber = "";
        }

        $scope.clearClaimantID = function () {
            $scope.SCB.pendingClaimantID = "";
        }

        $scope.clearStartAndEndDate = function () {
            $scope.SCB.pendingStartDate = "";
            $scope.SCB.pendingEndDate = "";
        }

        $scope.clearLowerAndUpperBoundClaimAmount = function () {
            $scope.SCB.pendingLowerBoundClaimAmount = "";
            $scope.SCB.pendingUpperBoundClaimAmount = "";
        }

        $scope.clearLowerAndUpperBoundICD = function () {
            $scope.SCB.pendingLowerBoundICD = "";
            $scope.SCB.pendingUpperBoundICD = "";
        }

        $scope.clearLowerAndUpperBoundCPT = function () {
            $scope.SCB.pendingLowerBoundCPT = "";
            $scope.SCB.pendingUpperBoundCPT = "";
        }

        $scope.clearAll = function () {
            $scope.SCB.pendingStartDate = "";
            $scope.SCB.pendingEndDate = "";
            $scope.SCB.pendingLowerBoundClaimAmount = "";
            $scope.SCB.pendingUpperBoundClaimAmount = "";
            $scope.SCB.pendingLowerBoundICD = "";
            $scope.SCB.pendingUpperBoundICD = "";
            $scope.SCB.pendingLowerBoundCPT = "";
            $scope.SCB.pendingUpperBoundCPT = "";
            $scope.SCB.pendingClaimNumber = "";
            $scope.SCB.pendingClaimantID = "";
        }

        $scope.DownloadClaimCostsResults = function () {
            window.open("/SweeperUI/DownloadClaimCostsResults", "_blank");
        }

        $scope.DownloadDiagnosesICDResults = function () {
            window.open("/SweeperUI/DownloadDiagnosesICDResults", "_blank");
        }

        $scope.DownloadDiagnosesICDClaimantsResults = function () {
            window.open("/SweeperUI/DownloadDiagnosesICDClaimantsResults", "_blank");
        }

        $scope.DownloadDiagnosesICDEventsResults = function () {
            window.open("/SweeperUI/DownloadDiagnosesICDEventsResults", "_blank");
        }

        $scope.DownloadDiagnosesICDClaimsResults = function () {
            window.open("/SweeperUI/DownloadDiagnosesICDClaimsResults", "_blank");
        }

        $scope.DownloadDiagnosesGroupResults = function () {
            window.open("/SweeperUI/DownloadDiagnosesGroupResults", "_blank");
        }

        $scope.DownloadDiagnosesSubgroupResults = function () {
            window.open("/SweeperUI/DownloadDiagnosesSubgroupResults", "_blank");
        }

        $scope.DownloadProvidersResults = function () {
            window.open("/SweeperUI/DownloadProvidersResults", "_blank");
        }

        $scope.DownloadProvidersClaimsResults = function () {
            window.open("/SweeperUI/DownloadProvidersClaimsResults", "_blank");
        }

        $scope.DownloadClaimantsResults = function () {
            window.open("/SweeperUI/DownloadClaimantsResults", "_blank");
        }

        $scope.DownloadProceduresResults = function () {
            window.open("/SweeperUI/DownloadProceduresCodeResults", "_blank");
        }

        $scope.DownloadProceduresClaimsResults = function () {
            window.open("/SweeperUI/DownloadProceduresCodeClaimsResults", "_blank");
        }

        $scope.DownloadProceduresGroupResults = function () {
            window.open("/SweeperUI/DownloadProceduresGroupResults", "_blank");
        }

        $scope.DownloadProceduresSubgroupResults = function () {
            window.open("/SweeperUI/DownloadProceduresSubgroupResults", "_blank");
        }

        $scope.GetClaimCostsResults = function () {
            $http({
                url: '/SweeperUI/GetClaimsResults',
                method: 'POST',
                data: { startDate: $scope.SCB.startDate, endDate: $scope.SCB.endDate, lowerBoundICD: $scope.SCB.lowerBoundICD, upperBoundICD: $scope.SCB.upperBoundICD, lowerBoundCPT: $scope.SCB.lowerBoundCPT, upperBoundCPT: $scope.SCB.upperBoundCPT, lowerBoundClaimAmount: $scope.SCB.lowerBoundClaimAmount, upperBoundClaimAmount: $scope.SCB.upperBoundClaimAmount, claimantID: $scope.SCB.claimantID, claimNumber: $scope.SCB.claimNumber }
            }).success(function (data, status, headers, config) {
                $scope.SCB.displayState = 'main';
                $scope.SCB.ClaimCostsResults = data;
                $scope.buildClaimCostsPages();
            }).error(function (data, status, headers, config) {
            });
        }

        $scope.GetDiagnosesGroupResults = function () {
            $http({
                url: '/SweeperUI/GetDiagnosesGroupResults',
                method: 'POST',
                data: { startDate: $scope.SCB.startDate, endDate: $scope.SCB.endDate, lowerBoundICD: $scope.SCB.lowerBoundICD, upperBoundICD: $scope.SCB.upperBoundICD, lowerBoundCPT: $scope.SCB.lowerBoundCPT, upperBoundCPT: $scope.SCB.upperBoundCPT, lowerBoundClaimAmount: $scope.SCB.lowerBoundClaimAmount, upperBoundClaimAmount: $scope.SCB.upperBoundClaimAmount, claimantID: $scope.SCB.claimantID, claimNumber: $scope.SCB.claimNumber }
            }).success(function (data, status, headers, config) {
                $scope.SCB.displayState = 'main';
                $scope.SCB.DiagnosesGroupResults = data;
                for (var i = 0; i < $scope.SCB.DiagnosesGroupResults.length; i++) {
                    $scope.SCB.DiagnosesGroupResults[i].PMPMRaw = $scope.SCB.DiagnosesGroupResults[i].ClaimTotalRaw / $scope.SCB.membersCountFiltered / $scope.SCB.monthsFiltered;
                    $scope.SCB.DiagnosesGroupResults[i].PMPM = "$" + $scope.SCB.DiagnosesGroupResults[i].PMPMRaw.toFixed(2).replace(/(\d)(?=(\d{3})+(?!\d))/g, "$1,");
                }
                $scope.buildDiagnosesGroupPages();
            }).error(function (data, status, headers, config) {
            });
        }

        $scope.GetDiagnosesSubgroupResults = function () {
            $http({
                url: '/SweeperUI/GetDiagnosesSubgroupResults',
                method: 'POST',
                data: { startDate: $scope.SCB.startDate, endDate: $scope.SCB.endDate, lowerBoundICD: $scope.SCB.lowerBoundICD, upperBoundICD: $scope.SCB.upperBoundICD, lowerBoundCPT: $scope.SCB.lowerBoundCPT, upperBoundCPT: $scope.SCB.upperBoundCPT, lowerBoundClaimAmount: $scope.SCB.lowerBoundClaimAmount, upperBoundClaimAmount: $scope.SCB.upperBoundClaimAmount, claimantID: $scope.SCB.claimantID, claimNumber: $scope.SCB.claimNumber }
            }).success(function (data, status, headers, config) {
                $scope.SCB.displayState = 'main';
                $scope.SCB.DiagnosesSubgroupResults = data;
                for (var i = 0; i < $scope.SCB.DiagnosesSubgroupResults.length; i++) {
                    $scope.SCB.DiagnosesSubgroupResults[i].PMPMRaw = $scope.SCB.DiagnosesSubgroupResults[i].ClaimTotalRaw / $scope.SCB.membersCountFiltered / $scope.SCB.monthsFiltered;
                    $scope.SCB.DiagnosesSubgroupResults[i].PMPM = "$" + $scope.SCB.DiagnosesSubgroupResults[i].PMPMRaw.toFixed(2).replace(/(\d)(?=(\d{3})+(?!\d))/g, "$1,");
                }
                $scope.buildDiagnosesSubgroupPages();
            }).error(function (data, status, headers, config) {
            });
        }

        $scope.GetDiagnosesICDResults = function () {
            $http({
                url: '/SweeperUI/GetDiagnosesICDResults',
                method: 'POST',
                data: { startDate: $scope.SCB.startDate, endDate: $scope.SCB.endDate, lowerBoundICD: $scope.SCB.lowerBoundICD, upperBoundICD: $scope.SCB.upperBoundICD, lowerBoundCPT: $scope.SCB.lowerBoundCPT, upperBoundCPT: $scope.SCB.upperBoundCPT, lowerBoundClaimAmount: $scope.SCB.lowerBoundClaimAmount, upperBoundClaimAmount: $scope.SCB.upperBoundClaimAmount, claimantID: $scope.SCB.claimantID, claimNumber: $scope.SCB.claimNumber }
            }).success(function (data, status, headers, config) {
                $scope.SCB.displayState = 'main';
                $scope.SCB.DiagnosesICDResults = data;
                $scope.buildDiagnosesICDPages();
            }).error(function (data, status, headers, config) {
            });
        }

        $scope.GetDiagnosesICDClaimantsResults = function () {
            $http({
                url: '/SweeperUI/GetClaimantsResults',
                method: 'POST',
                data: { startDate: $scope.SCB.startDate, endDate: $scope.SCB.endDate, lowerBoundICD: $scope.SCB.lowerBoundICD, upperBoundICD: $scope.SCB.upperBoundICD, lowerBoundCPT: $scope.SCB.lowerBoundCPT, upperBoundCPT: $scope.SCB.upperBoundCPT, lowerBoundClaimAmount: $scope.SCB.lowerBoundClaimAmount, upperBoundClaimAmount: $scope.SCB.upperBoundClaimAmount, claimantID: $scope.SCB.claimantID, claimNumber: $scope.SCB.claimNumber }
            }).success(function (data, status, headers, config) {
                $scope.SCB.displayState = 'main';
                $scope.SCB.DiagnosesICDClaimantsResults = data;
                $scope.buildDiagnosesICDClaimantsPages();
            }).error(function (data, status, headers, config) {
            });
        }

        $scope.GetDiagnosesICDEventsResults = function () {
            $http({
                url: '/SweeperUI/GetDiagnosesICDEventsResults',
                method: 'POST',
                data: { startDate: $scope.SCB.startDate, endDate: $scope.SCB.endDate, lowerBoundICD: $scope.SCB.lowerBoundICD, upperBoundICD: $scope.SCB.upperBoundICD, lowerBoundCPT: $scope.SCB.lowerBoundCPT, upperBoundCPT: $scope.SCB.upperBoundCPT, lowerBoundClaimAmount: $scope.SCB.lowerBoundClaimAmount, upperBoundClaimAmount: $scope.SCB.upperBoundClaimAmount, claimantID: $scope.SCB.claimantID, claimNumber: $scope.SCB.claimNumber }
            }).success(function (data, status, headers, config) {
                $scope.SCB.displayState = 'main';
                $scope.SCB.DiagnosesICDEventsResults = data;
                $scope.buildDiagnosesICDEventsPages();
            }).error(function (data, status, headers, config) {
            });
        }

        $scope.GetDiagnosesICDClaimsResults = function () {
            $http({
                url: '/SweeperUI/GetClaimsResults',
                method: 'POST',
                data: { startDate: $scope.SCB.startDate, endDate: $scope.SCB.endDate, lowerBoundICD: $scope.SCB.lowerBoundICD, upperBoundICD: $scope.SCB.upperBoundICD, lowerBoundCPT: $scope.SCB.lowerBoundCPT, upperBoundCPT: $scope.SCB.upperBoundCPT, lowerBoundClaimAmount: $scope.SCB.lowerBoundClaimAmount, upperBoundClaimAmount: $scope.SCB.upperBoundClaimAmount, claimantID: $scope.SCB.claimantID, claimNumber: $scope.SCB.claimNumber }
            }).success(function (data, status, headers, config) {
                $scope.SCB.displayState = 'main';
                $scope.SCB.DiagnosesICDClaimsResults = data;
                $scope.buildDiagnosesICDClaimsPages();
            }).error(function (data, status, headers, config) {
            });
        }

        $scope.GetProvidersResults = function () {
            $http({
                url: '/SweeperUI/GetProvidersResults',
                method: 'POST',
                data: { startDate: $scope.SCB.startDate, endDate: $scope.SCB.endDate, lowerBoundClaimAmount: $scope.SCB.lowerBoundClaimAmount, upperBoundClaimAmount: $scope.SCB.upperBoundClaimAmount, providerNPI: $scope.SCB.providerNPI }
            }).success(function (data, status, headers, config) {
                $scope.SCB.displayState = 'main';
                $scope.SCB.ProvidersResults = data;
                $scope.buildProvidersPages();
            }).error(function (data, status, headers, config) {
            });
        }

        $scope.GetProvidersClaimsResults = function () {
            $http({
                url: '/SweeperUI/GetProvidersClaimsResults',
                method: 'POST',
                data: { startDate: $scope.SCB.startDate, endDate: $scope.SCB.endDate, lowerBoundICD: $scope.SCB.lowerBoundICD, upperBoundICD: $scope.SCB.upperBoundICD, lowerBoundCPT: $scope.SCB.lowerBoundCPT, upperBoundCPT: $scope.SCB.upperBoundCPT, lowerBoundClaimAmount: $scope.SCB.lowerBoundClaimAmount, upperBoundClaimAmount: $scope.SCB.upperBoundClaimAmount, claimantID: $scope.SCB.claimantID, claimNumber: $scope.SCB.claimNumber }
            }).success(function (data, status, headers, config) {
                $scope.SCB.displayState = 'main';
                $scope.SCB.ProvidersClaimsResults = data;
                $scope.buildProvidersClaimsPages();
            }).error(function (data, status, headers, config) {
            });
        }

        $scope.GetProceduresCodeResults = function () {
            $http({
                url: '/SweeperUI/GetProceduresCodeResults',
                method: 'POST',
                data: { startDate: $scope.SCB.startDate, endDate: $scope.SCB.endDate, lowerBoundICD: $scope.SCB.lowerBoundICD, upperBoundICD: $scope.SCB.upperBoundICD, lowerBoundCPT: $scope.SCB.lowerBoundCPT, upperBoundCPT: $scope.SCB.upperBoundCPT, lowerBoundClaimAmount: $scope.SCB.lowerBoundClaimAmount, upperBoundClaimAmount: $scope.SCB.upperBoundClaimAmount, claimantID: $scope.SCB.claimantID, claimNumber: $scope.SCB.claimNumber }
            }).success(function (data, status, headers, config) {
                $scope.SCB.displayState = 'main';
                $scope.SCB.ProceduresResults = data;
                $scope.buildProceduresPages();
            }).error(function (data, status, headers, config) {
            });
        }

        $scope.GetProceduresCodeClaimsResults = function () {
            $http({
                url: '/SweeperUI/GetProceduresCodeClaimsResults',
                method: 'POST',
                data: { startDate: $scope.SCB.startDate, endDate: $scope.SCB.endDate, lowerBoundICD: $scope.SCB.lowerBoundICD, upperBoundICD: $scope.SCB.upperBoundICD, lowerBoundCPT: $scope.SCB.lowerBoundCPT, upperBoundCPT: $scope.SCB.upperBoundCPT, lowerBoundClaimAmount: $scope.SCB.lowerBoundClaimAmount, upperBoundClaimAmount: $scope.SCB.upperBoundClaimAmount, claimantID: $scope.SCB.claimantID, claimNumber: $scope.SCB.claimNumber }
            }).success(function (data, status, headers, config) {
                $scope.SCB.displayState = 'main';
                $scope.SCB.ProceduresClaimsResults = data;
                $scope.buildProceduresClaimsPages();
            }).error(function (data, status, headers, config) {
            });
        }

        $scope.GetProceduresGroupResults = function () {
            $http({
                url: '/SweeperUI/GetProceduresGroupResults',
                method: 'POST',
                data: { startDate: $scope.SCB.startDate, endDate: $scope.SCB.endDate, lowerBoundICD: $scope.SCB.lowerBoundICD, upperBoundICD: $scope.SCB.upperBoundICD, lowerBoundCPT: $scope.SCB.lowerBoundCPT, upperBoundCPT: $scope.SCB.upperBoundCPT, lowerBoundClaimAmount: $scope.SCB.lowerBoundClaimAmount, upperBoundClaimAmount: $scope.SCB.upperBoundClaimAmount, claimantID: $scope.SCB.claimantID, claimNumber: $scope.SCB.claimNumber }
            }).success(function (data, status, headers, config) {
                $scope.SCB.displayState = 'main';
                $scope.SCB.ProceduresGroupResults = data;
                $scope.buildProceduresGroupPages();
            }).error(function (data, status, headers, config) {
            });
        }

        $scope.GetProceduresSubgroupResults = function () {
            $http({
                url: '/SweeperUI/GetProceduresSubgroupResults',
                method: 'POST',
                data: { startDate: $scope.SCB.startDate, endDate: $scope.SCB.endDate, lowerBoundICD: $scope.SCB.lowerBoundICD, upperBoundICD: $scope.SCB.upperBoundICD, lowerBoundCPT: $scope.SCB.lowerBoundCPT, upperBoundCPT: $scope.SCB.upperBoundCPT, lowerBoundClaimAmount: $scope.SCB.lowerBoundClaimAmount, upperBoundClaimAmount: $scope.SCB.upperBoundClaimAmount, claimantID: $scope.SCB.claimantID, claimNumber: $scope.SCB.claimNumber }
            }).success(function (data, status, headers, config) {
                $scope.SCB.displayState = 'main';
                $scope.SCB.ProceduresSubgroupResults = data;
                $scope.buildProceduresSubgroupPages();
            }).error(function (data, status, headers, config) {
            });
        }

        $scope.GetClaimantsResults = function () {
            $http({
                url: '/SweeperUI/GetClaimantsResults',
                method: 'POST',
                data: { startDate: $scope.SCB.startDate, endDate: $scope.SCB.endDate, lowerBoundICD: $scope.SCB.lowerBoundICD, upperBoundICD: $scope.SCB.upperBoundICD, lowerBoundCPT: $scope.SCB.lowerBoundCPT, upperBoundCPT: $scope.SCB.upperBoundCPT, lowerBoundClaimAmount: $scope.SCB.lowerBoundClaimAmount, upperBoundClaimAmount: $scope.SCB.upperBoundClaimAmount, claimantID: $scope.SCB.claimantID, claimNumber: $scope.SCB.claimNumber }
            }).success(function (data, status, headers, config) {
                $scope.SCB.displayState = 'main';
                $scope.SCB.ClaimantsResults = data;
                $scope.buildClaimantsPages();
            }).error(function (data, status, headers, config) {
            });
        }

        // User Management System interface
        $scope.getClaims = function () {
            $http({
                url: '/SweeperUI/GetUserClaims',
                method: 'GET'
            }).success(function (data, status, headers, config) {
                $scope.claims.full = data.claims;
                $scope.claims.access = {
                    master: data.master,
                    dev: data.dev,
                    admin: data.admin,
                    dms: data.dms
                };
            }).error(function (data, status, headers, config) {
            });
        }

        $scope.GetFullClaimsSummary = function () {
            $http({
                url: '/SweeperUI/GetFullClaimsSummary',
                method: 'POST'
            }).success(function (data, status, headers, config) {
                $scope.SCB.displayState = 'main';
                $scope.SCB.FullClaimsSummary = data;
                $scope.SCB.claimsCountFull = $scope.SCB.FullClaimsSummary.Count;
                $scope.SCB.claimsTotalFull = $scope.SCB.FullClaimsSummary.Total;
                $scope.SCB.claimsMaximumFull = $scope.SCB.FullClaimsSummary.Maximum;
                $scope.SCB.claimsAverageFull = $scope.SCB.FullClaimsSummary.Average;
                $scope.SCB.membersCountFull = $scope.SCB.FullClaimsSummary.MemberCount;
                $scope.SCB.employeesCountFull = $scope.SCB.FullClaimsSummary.EmployeeCount;
                $scope.SCB.nonemployeesCountFull = $scope.SCB.FullClaimsSummary.MemberCount - $scope.SCB.employeesCountFull;
                $scope.SCB.claimsCountFiltered = $scope.SCB.FullClaimsSummary.Count;
                $scope.SCB.claimsTotalFiltered = $scope.SCB.FullClaimsSummary.Total;
                $scope.SCB.claimsMaximumFiltered = $scope.SCB.FullClaimsSummary.Maximum;
                $scope.SCB.claimsAverageFiltered = $scope.SCB.FullClaimsSummary.Average;
                $scope.SCB.membersCountFiltered = $scope.SCB.FullClaimsSummary.MemberCount;
                $scope.SCB.employeesCountFiltered = $scope.SCB.FullClaimsSummary.EmployeeCount;
                $scope.SCB.nonemployeesCountFiltered = $scope.SCB.FullClaimsSummary.MemberCount - $scope.SCB.employeesCountFiltered;
                $scope.SCB.monthsFiltered = 12;
            }).error(function (data, status, headers, config) {
            });
        }

        $scope.GetFilteredClaimsSummary = function () {
            if ($scope.SCB.startDate != "" ||
                $scope.SCB.endDate != "")
            {
                $http({
                    url: '/SweeperUI/GetFilteredClaimsSummary',
                    method: 'POST',
                    data: { startDate: $scope.SCB.startDate, endDate: $scope.SCB.endDate }
                }).success(function (data, status, headers, config) {
                    $scope.SCB.displayState = 'main';
                    $scope.SCB.FilteredClaimsSummary = data;
                    $scope.SCB.claimsCountFiltered = $scope.SCB.FilteredClaimsSummary.Count;
                    $scope.SCB.claimsTotalFiltered = $scope.SCB.FilteredClaimsSummary.Total;
                    $scope.SCB.claimsMaximumFiltered = $scope.SCB.FilteredClaimsSummary.Maximum;
                    $scope.SCB.claimsAverageFiltered = $scope.SCB.FilteredClaimsSummary.Average;
                    $scope.SCB.membersCountFiltered = $scope.SCB.FilteredClaimsSummary.MemberCount;
                    $scope.SCB.employeesCountFiltered = $scope.SCB.FilteredClaimsSummary.EmployeeCount;
                    $scope.SCB.nonemployeesCountFiltered = $scope.SCB.FilteredClaimsSummary.MemberCount - $scope.SCB.employeesCountFiltered;
                }).error(function (data, status, headers, config) {
                });
            }
        }

        $scope.GetFullClaimsSummary();

        // Utility functions
        $scope.getCurrentYear = function () {
            return moment(new Date()).get("year");
        }
        $scope.isThisYear = function (momentDate) {
            return $scope.getCurrentYear() == momentDate.get("year");
        }
        $scope.isToday = function (momentDate) {
            var today = moment(new Date());
            return today.diff(momentDate, "days") == 0;
        }
        $scope.windowKeyDown = function(t) {
            if (t.keyCode == 27) {
                $scope.SCB.displayState = "main";
            }
        }

        // Click to drill down functions
        $scope.clickProceduresGroup = function (procedureGroup) {
            var stringProcedureGroup = procedureGroup.ProcedureGroup;
            var hyphenIndex = stringProcedureGroup.indexOf("-");
            $scope.SCB.lowerBoundCPT = stringProcedureGroup.substr(0, hyphenIndex);
            $scope.SCB.upperBoundCPT = stringProcedureGroup.substr(hyphenIndex + 1);
            $scope.SCB.displayState = "main";
            $scope.tabSelectProceduresSubgroup();
            $scope.SCB.ProceduresGroup.drillShow = false;
            $scope.SCB.ProceduresSubgroup.drillShow = true;
        }

        $scope.clickProceduresSubgroup = function (procedureSubgroup) {
            var stringProcedureSubgroup = procedureSubgroup.ProcedureSubgroup;
            var hyphenIndex = stringProcedureSubgroup.indexOf("-");
            $scope.SCB.lowerBoundCPT = stringProcedureSubgroup.substr(0, hyphenIndex);
            $scope.SCB.upperBoundCPT = stringProcedureSubgroup.substr(hyphenIndex + 1);
            $scope.SCB.displayState = "main";
            $scope.tabSelectProcedures();
            $scope.SCB.ProceduresSubgroup.drillShow = false;
            $scope.SCB.Procedures.drillShow = true;
        }

        $scope.clickProceduresCode = function (procedure) {
            $scope.SCB.lowerBoundCPT = procedure.ProcedureCode;
            $scope.SCB.upperBoundCPT = procedure.ProcedureCode;
            $scope.SCB.displayState = "main";
            $scope.tabSelectProceduresClaims();
            $scope.SCB.Procedures.drillShow = false;
            $scope.SCB.ProceduresClaims.drillShow = true;
        }

        $scope.clickDiagnosesGroup = function (diagnosisGroup) {
            var stringDiagnosticGroup = diagnosisGroup.DiagnosticGroup;
            var hyphenIndex = stringDiagnosticGroup.indexOf("-");
            $scope.SCB.lowerBoundICD = stringDiagnosticGroup.substr(0, hyphenIndex);
            $scope.SCB.upperBoundICD = stringDiagnosticGroup.substr(hyphenIndex + 1);
            $scope.SCB.displayState = "main";
            $scope.tabSelectDiagnosesBySubgroup();
            $scope.SCB.DiagnosesGroup.drillShow = false;
            $scope.SCB.DiagnosesSubgroup.drillShow = true;
        }

        $scope.clickDiagnosesSubgroup = function (diagnosisSubgroup) {
            var stringDiagnosticSubgroup = diagnosisSubgroup.DiagnosticSubgroup;
            var hyphenIndex = stringDiagnosticSubgroup.indexOf("-");
            $scope.SCB.lowerBoundICD = stringDiagnosticSubgroup.substr(0, hyphenIndex);
            $scope.SCB.upperBoundICD = stringDiagnosticSubgroup.substr(hyphenIndex + 1);
            $scope.SCB.displayState = "main";
            $scope.tabSelectDiagnosesByICD();
            $scope.SCB.DiagnosesSubgroup.drillShow = false;
            $scope.SCB.DiagnosesICD.drillShow = true;
        }

        $scope.clickDiagnosesICD = function (diagnosisICD) {
            $scope.SCB.lowerBoundICD = diagnosisICD.ICDCode;
            $scope.SCB.upperBoundICD = diagnosisICD.ICDCode;
            $scope.SCB.displayState = "main";
            $scope.tabSelectDiagnosesByICDClaimants();
            $scope.SCB.DiagnosesICD.drillShow = false;
            $scope.SCB.DiagnosesICDClaimants.drillShow = true;
        }

        $scope.clickDiagnosesICDClaimant = function (claimant) {
            $scope.SCB.claimantID = claimant.MemberId;
            $scope.SCB.displayState = "main";
            $scope.tabSelectDiagnosesByICDEvents();
            $scope.SCB.DiagnosesICDClaimants.drillShow = false;
            $scope.SCB.DiagnosesICDEvents.drillShow = true;
        }

        $scope.clickDiagnosesICDEvent = function (event) {
            $scope.SCB.claimNumber = event.ClaimNumber;
            $scope.SCB.displayState = "main";
            $scope.tabSelectDiagnosesByICDClaims();
            $scope.SCB.DiagnosesICDEvents.drillShow = false;
            $scope.SCB.DiagnosesICDClaims.drillShow = true;
        }

        $scope.clickProvidersCode = function (provider) {
            $scope.SCB.providerNPI = provider.Npi;
            $scope.SCB.displayState = "main";
            $scope.tabSelectProvidersClaims();
            $scope.SCB.Providers.drillShow = false;
            $scope.SCB.ProvidersClaims.drillShow = true;
        }

        // Initial settings
        $scope.clickClaimCostsSort("StatementDate");
        $scope.buildClaimCostsPages();
        $scope.clickDiagnosesGroupSort("DiagnosticGroup");
        $scope.buildDiagnosesGroupPages();
        $scope.clickDiagnosesSubgroupSort("DiagnosticSubgroup");
        $scope.buildDiagnosesSubgroupPages();
        $scope.clickDiagnosesICDSort("ICDCode");
        $scope.buildDiagnosesICDPages();
        $scope.clickDiagnosesICDClaimantsSort("ICDCode");
        $scope.buildDiagnosesICDClaimantsPages();
        $scope.clickDiagnosesICDEventsSort("ICDCode");
        $scope.buildDiagnosesICDEventsPages();
        $scope.clickDiagnosesICDClaimsSort("ICDCode");
        $scope.buildDiagnosesICDClaimsPages();
        $scope.clickProvidersSort("FullName");
        $scope.buildProvidersPages();
        $scope.clickProvidersClaimsSort("FullName");
        $scope.buildProvidersClaimsPages();
        $scope.clickProceduresSort("ProcedureCode");
        $scope.buildProceduresPages();
        $scope.clickProceduresClaimsSort("ProcedureCode");
        $scope.buildProceduresClaimsPages();
        $scope.clickProceduresGroupSort("ProcedureGroup");
        $scope.buildProceduresGroupPages();
        $scope.clickProceduresSubgroupSort("ProcedureSubgroup");
        $scope.buildProceduresSubgroupPages();
        $scope.clickClaimantsSort("FullName");
        $scope.buildClaimantsPages();

        $(window).keydown($scope.windowKeyDown);

        $scope.getClaims();
    });