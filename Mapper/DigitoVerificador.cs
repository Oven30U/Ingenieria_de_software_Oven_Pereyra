namespace Mapper
{

    public static class DigitoVerificador
    {
        public static int CalcularModulo11(string codigoBase)
        {
            int suma = 0;
            int peso = 2;
            for (int i = codigoBase.Length - 1; i >= 0; i--)
            {
                int digito = codigoBase[i] - '0';
                suma += digito * peso;
                peso++;
                if (peso > 7) peso = 2;
            }
            int resto = suma % 11;
            int dv = 11 - resto;
            if (dv == 11) dv = 0;
            if (dv == 10) dv = 1;
            return dv;
        }

        public static string GenerarCodigoConDV(string codigoBase)
        {
            return codigoBase + CalcularModulo11(codigoBase).ToString();
        }

        public static bool Validar(string codigoCompleto)
        {
            if (string.IsNullOrEmpty(codigoCompleto) || codigoCompleto.Length < 2)
                return false;

            string baseCodigo = codigoCompleto.Substring(0, codigoCompleto.Length - 1);
            int dvIngresado = codigoCompleto[codigoCompleto.Length - 1] - '0';
            return CalcularModulo11(baseCodigo) == dvIngresado;
        }
    }
}
