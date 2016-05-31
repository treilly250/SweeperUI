function ClaimManager(claims) {
    this.claims = claims;

    this.Master = this.checkAccess(["master_access"]);
    this.DEV = this.checkAccess(["dev_access"]);
    this.DMS = this.checkAccess(["dms_access"]);
    this.USHC_Admin = this.checkAccess(["ushc_admin_access"]);
    this.UMS = this.checkAccess(["ums_access"]);
}

ClaimManager.prototype.getClaim = function (claimType) {
    for (var g = 0; g < this.claims.length; g++) {
        if (this.claims[g].Type.toLowerCase() == claimType.toLowerCase()) {
            return this.claims[g];
        }
    }

    return null;
}

ClaimManager.prototype.getClaimValue = function (claimType) {
    var claim = this.getClaim(claimType);

    if (claim == null) {
        return null;
    }

    return claim.Value;
}

ClaimManager.prototype.checkAccess = function (validClaims, checkVal) {

    if (checkVal == null) {
        checkVal = "true";
    }

    for (var g = 0; g < this.claims.length; g++) {
        for (var f = 0; f < validClaims.length; f++) {
            if (this.claims[g].Type.toLowerCase() == validClaims[f].toLowerCase()) {
                if (this.claims[g].Value == checkVal) {
                    return true;
                }
            }
        }
    }

    return false;
}