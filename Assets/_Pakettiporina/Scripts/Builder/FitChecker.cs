using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pakettiporina
{
    // Tarkistaa sopiiko auto paketille: onko autossa paketin vaatima osa.
    // EI estä ajamista — palauttaa vain tuloksen ja viestin UI:ta varten.
    public static class FitChecker
    {
        public struct Result
        {
            public bool fits;      // sopiiko auto keikkaan
            public string message; // nayttoteksti pelaajalle
        }

        public static Result Check(CarBuilder builder, PackageData pkg)
        {
            Result r = new Result();

            if (pkg == null)
            {
                r.fits = true;
                r.message = "Ei valittua pakettia";
                return r;
            }

            // Jos paketti ei vaadi mitaan osaa, se sopii aina.
            if (pkg.requiredPart == null)
            {
                r.fits = true;
                r.message = "Sopii keikkaan!";
                return r;
            }

            bool has = builder != null && builder.HasPart(pkg.requiredPart);
            r.fits = has;
            r.message = has ? "Sopii keikkaan!" : "Tarvitsee: " + pkg.requiredPart.displayName;
            return r;
        }
    }
}