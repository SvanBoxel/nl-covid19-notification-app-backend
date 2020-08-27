﻿// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Services
{
    public class CorrectingGaenSigFormat
    {
//
// Test generated with:
//
// set -e
// repeat 30 {
//    # Generate pub/keypare
//    openssl ecparam -name secp256k1 -genkey -noout -out ec.key
//    openssl ec -in ec.key -pubout -out ec.pub
//
//    # Generate payload to sign
//    openssl rand 1300 > payload
//
//    # Sign payload & check
//    #
//    openssl dgst -sha256 -sign ec.key payload > signature.bin
//    openssl dgst -sha256 -verify ec.pub -signature signature.bin payload
//
//    # Extract the two 32 byte integers; and output these concatenated
//    # much like .NET its cert.GetECDsaPrivateKey().SignData() does.
//
//    openssl asn1parse -inform DER -in signature.bin |\
//    	grep INTEGER |\
//    	sed -e 's/.*://' |\
//    	xxd -r -p - | base64 > signature.r64
//
//    echo \[DataRow\(\"`base64 signature.r64`\",\"`base64 signature.bin`\"\)\]
// }
        [Theory]
        [InlineData("NtnqKDHatsWQYS1TKjxD+73qQ5BWz2zAEWqzpFs8BCyQ+LtoaMxfxtsM0o/HVUMsce2YTNSQM39Dmf6+1h8UJA==", "MEUCIDbZ6igx2rbFkGEtUyo8Q/u96kOQVs9swBFqs6RbPAQsAiEAkPi7aGjMX8bbDNKPx1VDLHHtmEzUkDN/Q5n+vtYfFCQ=")]
        [InlineData("9CqMHwPaGnEOOW28vuaw9btDgwe7chpWmMMUJJFSGykTGJT9yk86fDPuFivi2S27FdOYiAwSIxvx1ZxLaly+Tw==", "MEUCIQD0KowfA9oacQ45bby+5rD1u0ODB7tyGlaYwxQkkVIbKQIgExiU/cpPOnwz7hYr4tktuxXTmIgMEiMb8dWcS2pcvk8=")]
        [InlineData("h0oJTLbvyE7jkfEhnqn5YIMI0g+K67s0OAqpj8Gb2tnIQEmw8qZ/M4tc0fcUIX0gjxyUqwTFO5nRUi8FN+UZaw==", "MEYCIQCHSglMtu/ITuOR8SGeqflggwjSD4rruzQ4CqmPwZva2QIhAMhASbDypn8zi1zR9xQhfSCPHJSrBMU7mdFSLwU35Rlr")]
        [InlineData("cuUOKUuBjdJQvGt0TaxJJSnd/ByYcgeim2ndEpzYfwakW6ZWDg+sD5Bxuw0va0Uhrjuwnx4toAS0kShQFHSHVA==", "MEUCIHLlDilLgY3SULxrdE2sSSUp3fwcmHIHoptp3RKc2H8GAiEApFumVg4PrA+QcbsNL2tFIa47sJ8eLaAEtJEoUBR0h1Q=")]
        [InlineData("YVrPWa3Qp+yKsorY+FNJz4bslSqRp80oO6B3lDKfFvupvPvaRNz3EkJAxRsfvLwb3W4D/yZvQbGzsK1m+pMjVg==", "MEUCIGFaz1mt0KfsirKK2PhTSc+G7JUqkafNKDugd5Qynxb7AiEAqbz72kTc9xJCQMUbH7y8G91uA/8mb0Gxs7CtZvqTI1Y=")]
        [InlineData("33tCqMxPZFj4rrNn+IvSqcg51jrhr76X9RzldzWbUJCYMcRQWPFQ9tylEbBhVTRuhhGniHMH5mYCSD4JzE8W7Q==", "MEYCIQDfe0KozE9kWPius2f4i9KpyDnWOuGvvpf1HOV3NZtQkAIhAJgxxFBY8VD23KURsGFVNG6GEaeIcwfmZgJIPgnMTxbt")]
        [InlineData("YYal1Qy7qeTWBuNBfCqEteuhaermnO8CyPNMA0adlBpY6kQmSVvLrkpi2Brp/x+5sAawmrkhfCVR0cs+mkDJoQ==", "MEQCIGGGpdUMu6nk1gbjQXwqhLXroWnq5pzvAsjzTANGnZQaAiBY6kQmSVvLrkpi2Brp/x+5sAawmrkhfCVR0cs+mkDJoQ==")]
        [InlineData("ezYW3tIkilAWZAKjgN1EbRFf8pQybIbXxNR4EmlQhN/74e81RFIxlgJ1lsB+I2HsVxqAMoRUfRQZCj5h0jCkzw==", "MEUCIHs2Ft7SJIpQFmQCo4DdRG0RX/KUMmyG18TUeBJpUITfAiEA++HvNURSMZYCdZbAfiNh7FcagDKEVH0UGQo+YdIwpM8=")]
        [InlineData("b0BUk787/qreW3FLJ+vpoYNAhQSnRvr1nPwM0KEnYeKiKTj5mMc0ceRBIs0F2v6p5Ku0vrZPWSp3hZE90exwpA==", "MEUCIG9AVJO/O/6q3ltxSyfr6aGDQIUEp0b69Zz8DNChJ2HiAiEAoik4+ZjHNHHkQSLNBdr+qeSrtL62T1kqd4WRPdHscKQ=")]
        [InlineData("4gWLhDGRYowlWihW8rKNuQBv59RdojGaoxUVjBkWzTb+lwCpVAQiFqlWANwo+KD1N8L+24qhcJVvezO7YmL0NQ==", "MEYCIQDiBYuEMZFijCVaKFbyso25AG/n1F2iMZqjFRWMGRbNNgIhAP6XAKlUBCIWqVYA3Cj4oPU3wv7biqFwlW97M7tiYvQ1")]
        [InlineData("b/PqJCuvjQMGgVTjV28c3HPg1w/wtdC3lf7eedl2DAkjX787S9xSq9pVuxtTaiAlP+w5mCbFHC6Y6ely422kdQ==", "MEQCIG/z6iQrr40DBoFU41dvHNxz4NcP8LXQt5X+3nnZdgwJAiAjX787S9xSq9pVuxtTaiAlP+w5mCbFHC6Y6ely422kdQ==")]
        [InlineData("i4y0DeLxbn4pj9V6AJnWizuLcWUVvslcwcyB6OK5dhzKpmgNkyQxxZygOTA5xKqQExYGjvjwCJfABa3dm98ncw==", "MEYCIQCLjLQN4vFufimP1XoAmdaLO4txZRW+yVzBzIHo4rl2HAIhAMqmaA2TJDHFnKA5MDnEqpATFgaO+PAIl8AFrd2b3ydz")]
        [InlineData("XobW1IkxvURGdXKjKhEJxyWfnaPAU8SrLA2oehJpSCt8ghUqT9q4jvne4PvdeiJItFjDybtSSEfeKjJM8+dbAw==", "MEQCIF6G1tSJMb1ERnVyoyoRCccln52jwFPEqywNqHoSaUgrAiB8ghUqT9q4jvne4PvdeiJItFjDybtSSEfeKjJM8+dbAw==")]
        [InlineData("eWKlGrg7O+TdqdZThoMFdlCftZQebGTDUrRQyLCLw4wEBFkwHL/d9GA/F/qKAEvN4UdcSa4xmYgbeofzs9DIZw==", "MEQCIHlipRq4Ozvk3anWU4aDBXZQn7WUHmxkw1K0UMiwi8OMAiAEBFkwHL/d9GA/F/qKAEvN4UdcSa4xmYgbeofzs9DIZw==")]
        [InlineData("e1Dv/Av9OPoDpyzPjMjJDouYQTTsI+TokhLEsk58S0BadVOJmkk6vFJro90aIQEHDdPy470JC1RQapoG9KRgSw==", "MEQCIHtQ7/wL/Tj6A6csz4zIyQ6LmEE07CPk6JISxLJOfEtAAiBadVOJmkk6vFJro90aIQEHDdPy470JC1RQapoG9KRgSw==")]
        [InlineData("eOKYU5eS1h5OfJVQZhsgs6Bn3UxONYQMMhw4eyNlfib+KfNpCXM6URVlAIts8z7bquraJ5BEFIQb9oAPUAlgsA==", "MEUCIHjimFOXktYeTnyVUGYbILOgZ91MTjWEDDIcOHsjZX4mAiEA/inzaQlzOlEVZQCLbPM+26rq2ieQRBSEG/aAD1AJYLA=")]
        [InlineData("Bi760/CUNXjjC0ksv9wOWnnUVvKxCtIXuZYniKLTscTZGLieBT3vqaFBfMvNYPZ/2ex+8ASNACqascQQ6lO3iQ==", "MEUCIAYu+tPwlDV44wtJLL/cDlp51FbysQrSF7mWJ4ii07HEAiEA2Ri4ngU976mhQXzLzWD2f9nsfvAEjQAqmrHEEOpTt4k=")]
        [InlineData("wZ0yb4fBTEP+b9kVkIm5fwyElVx+xBDaARhLwS+eHHnVliM0ys+b8DF8wRlaCYe1cb0Cnr8Hu79z2P6+1k14aA==", "MEYCIQDBnTJvh8FMQ/5v2RWQibl/DISVXH7EENoBGEvBL54ceQIhANWWIzTKz5vwMXzBGVoJh7VxvQKevwe7v3PY/r7WTXho")]
        [InlineData("ip6PHv9cSJAtmPqhS4C11S+pQg28gAf+qLV86uqoMmBFI9K2fg1oB1yHckk9BH7GcVVZ6eK2wUNjpuTN3+gDIA==", "MEUCIQCKno8e/1xIkC2Y+qFLgLXVL6lCDbyAB/6otXzq6qgyYAIgRSPStn4NaAdch3JJPQR+xnFVWenitsFDY6bkzd/oAyA=")]
        [InlineData("CGZaEdJYR3qQfzNPFtTzMsTSWguKZKUMIL21exB/Pj982wVUUjeFIPF0N18o79XyxTALX63MG/sqXlszdbK6iw==", "MEQCIAhmWhHSWEd6kH8zTxbU8zLE0loLimSlDCC9tXsQfz4/AiB82wVUUjeFIPF0N18o79XyxTALX63MG/sqXlszdbK6iw==")]
        [InlineData("xE796HUvyKf7ektGOIe9DHjMxv7zId+W2mL/L6FUiw2qRvoaPd8lqVVxRZo0Wbcjd5qgYWBOJ90EezkyXdzbww==", "MEYCIQDETv3odS/Ip/t6S0Y4h70MeMzG/vMh35baYv8voVSLDQIhAKpG+ho93yWpVXFFmjRZtyN3mqBhYE4n3QR7OTJd3NvD")]
        [InlineData("ppxzFwi+inTPZkRbjcvGvtIs2AYlXNW2d/4oEe5QqlYNcczx9yvOjjrrksyVHfM7NhrM2KsXFKrJ5Fecjbix7w==", "MEUCIQCmnHMXCL6KdM9mRFuNy8a+0izYBiVc1bZ3/igR7lCqVgIgDXHM8fcrzo4665LMlR3zOzYazNirFxSqyeRXnI24se8=")]
        [InlineData("XBQGWbYCVW+y/LXUohBifKgl1Wf5nP8p+7CsdBwP7+1IvxwnK2OsB61cBJtTXSx7HqjsnZRYfqRNWzOQ33S4AQ==", "MEQCIFwUBlm2AlVvsvy11KIQYnyoJdVn+Zz/KfuwrHQcD+/tAiBIvxwnK2OsB61cBJtTXSx7HqjsnZRYfqRNWzOQ33S4AQ==")]
        [InlineData("TuiwSQIk5142JPKO/vmY9J95i/rpHCp/5QEINykrwHAvKtg4CCSiAlST8RPA9V33Il+8IxYjaE7jIjUK8b5++A==", "MEQCIE7osEkCJOdeNiTyjv75mPSfeYv66Rwqf+UBCDcpK8BwAiAvKtg4CCSiAlST8RPA9V33Il+8IxYjaE7jIjUK8b5++A==")]
        [InlineData("sTPdPYsSONNeuav0xUX4zjBjICcB8VcOjRXOma6vxMBYu0jNgqYno/WGIYVNYCXL6oXv7rrzoc4tWieiEXcfzQ==", "MEUCIQCxM909ixI40165q/TFRfjOMGMgJwHxVw6NFc6Zrq/EwAIgWLtIzYKmJ6P1hiGFTWAly+qF7+6686HOLVonohF3H80=")]
        [InlineData("nOz+m2c/YhugAcB9GRQ7rOrx5987MkGjs5MRyMKZv5hP/3FhiNChxOqngC9yI/rcuEq8IOZ2zTh6h6kuEPOYxg==", "MEUCIQCc7P6bZz9iG6ABwH0ZFDus6vHn3zsyQaOzkxHIwpm/mAIgT/9xYYjQocTqp4AvciP63LhKvCDmds04eoepLhDzmMY=")]
        [InlineData("Xmq7xhGL55hBhxkZyscMH9DYLFObiXApZNyLXD7+5wd865k9oJ9c8TJ2MvzrYjt5kAa9DwBsv0006Wo1Xalsfg==", "MEQCIF5qu8YRi+eYQYcZGcrHDB/Q2CxTm4lwKWTci1w+/ucHAiB865k9oJ9c8TJ2MvzrYjt5kAa9DwBsv0006Wo1Xalsfg==")]
        [InlineData("Unekf6vyR2kGhk9tSvCtHIVjsNZ/PksSr6TiMljcIijSj4murXZbMTqrKwYCZvaP3L0eOv86Dqgz9lIoNGnFkg==", "MEUCIFJ3pH+r8kdpBoZPbUrwrRyFY7DWfz5LEq+k4jJY3CIoAiEA0o+Jrq12WzE6qysGAmb2j9y9Hjr/Og6oM/ZSKDRpxZI=")]
        [InlineData("idQoEkc+fvqZ9RPr7VAaZBvDzIiqjBQ6iuDbaYwXfQmV4yS4ZTWZ5jEQ83lSBEqzyi0vlAj+PIo65hDo/bFC6w==", "MEYCIQCJ1CgSRz5++pn1E+vtUBpkG8PMiKqMFDqK4NtpjBd9CQIhAJXjJLhlNZnmMRDzeVIESrPKLS+UCP48ijrmEOj9sULr")]
        [InlineData("S/82nozqc0CDxw4dVU8hv82DQts/Vk3weXgIxRLqmapf6ayOAROnR7jtzAWaZEkuIH+HlYuOUInOZgNBd9ckfQ==", "MEQCIEv/Np6M6nNAg8cOHVVPIb/Ng0LbP1ZN8Hl4CMUS6pmqAiBf6ayOAROnR7jtzAWaZEkuIH+HlYuOUInOZgNBd9ckfQ==")]
        [InlineData("itbYr0Fn+Bch+uZnw4MZO4WQa4/B7iL2+9Jo+Uby4+KGyt68R6mMfzYyRaFnFxP84+goq2wYaMwsYCNK31r8IQ==", "MEYCIQCK1tivQWf4FyH65mfDgxk7hZBrj8HuIvb70mj5RvLj4gIhAIbK3rxHqYx/NjJFoWcXE/zj6CirbBhozCxgI0rfWvwh")]
        [InlineData("2w06NNLSoQSK2wxSAj0bfZSbneRHVTBPwf5eJhsPwaxFt30dw5wxlw1kFHFaYHGX6MQdj1udK4AKRwr6o7410Q==", "MEUCIQDbDTo00tKhBIrbDFICPRt9lJud5EdVME/B/l4mGw/BrAIgRbd9HcOcMZcNZBRxWmBxl+jEHY9bnSuACkcK+qO+NdE=")]
        [InlineData("1s6Fde6FvTJ3IfrbBmXy6hyoDhWrhZz09Gs6u4d0O6K3xd1YpRKQG9o2NJVujf0lGcY+G19cqRI5DKABNwDCww==", "MEYCIQDWzoV17oW9Mnch+tsGZfLqHKgOFauFnPT0azq7h3Q7ogIhALfF3VilEpAb2jY0lW6N/SUZxj4bX1ypEjkMoAE3AMLD")]
        [InlineData("OhrcwQWwmv0BAGEYsfA5SeR5khJCpBkiQzkGQi5owt+LsKz+edVVC2RtNYFcT4GEa9i84ucjSfWL64q23qCH7g==", "MEUCIDoa3MEFsJr9AQBhGLHwOUnkeZISQqQZIkM5BkIuaMLfAiEAi7Cs/nnVVQtkbTWBXE+BhGvYvOLnI0n1i+uKtt6gh+4=")]
        [InlineData("9IRmu7rxT5L6iivjvEkk54LnEHnV9/F4HgbT8JqdYuk7yzM4ett3XSpBA9uTlEitt1dxxgc2eHJdWQcSrzCJDw==", "MEUCIQD0hGa7uvFPkvqKK+O8SSTngucQedX38XgeBtPwmp1i6QIgO8szOHrbd10qQQPbk5RIrbdXccYHNnhyXVkHEq8wiQ8=")]
        [InlineData("wlDwRmZAuUcKjfOOCXv2GBCJDs9cKt0ZOAaQCA3MNQdZ+23JMYTlc6J5oQKW7bKNxgbkyb7rVWNMXYB2P93c+g==", "MEUCIQDCUPBGZkC5RwqN844Je/YYEIkOz1wq3Rk4BpAIDcw1BwIgWfttyTGE5XOieaEClu2yjcYG5Mm+61VjTF2Adj/d3Po=")]
        [InlineData("n74WypaIKrdKPeZ7ipKFq2Udyn29KkR45N3BYlyz9PtctzhUFlHqtGiCa9Z/coKBBP0oAZiB3NmVcqEdXOUT1A==", "MEUCIQCfvhbKlogqt0o95nuKkoWrZR3Kfb0qRHjk3cFiXLP0+wIgXLc4VBZR6rRogmvWf3KCgQT9KAGYgdzZlXKhHVzlE9Q=")]
        [InlineData("T3LMgffOoeblbfuft4RzVprzPqoBfNBtMf22cHB7wFYaoYMcqKImXoPjKlh121Rlx0QDu+1Zi4S2GrtN0TZ4MQ==", "MEQCIE9yzIH3zqHm5W37n7eEc1aa8z6qAXzQbTH9tnBwe8BWAiAaoYMcqKImXoPjKlh121Rlx0QDu+1Zi4S2GrtN0TZ4MQ==")]
        [InlineData("/clA3rP4itLePsdFkWB+J+1K5DiI5kskLkvJm3pyVWS5WclbFFzdACLcfpztKEmHxo2d50zz3G8kJwR5V9lcjw==", "MEYCIQD9yUDes/iK0t4+x0WRYH4n7UrkOIjmSyQuS8mbenJVZAIhALlZyVsUXN0AItx+nO0oSYfGjZ3nTPPcbyQnBHlX2VyP")]
        [InlineData("Nk8htJPR4fUikD+M/HX6tirDjwYChoHbEEb64a3TdqvxXsgdE2fEzhkR+2+mmH5J6gu1HkmUI/FHrpG01/evQw==", "MEUCIDZPIbST0eH1IpA/jPx1+rYqw48GAoaB2xBG+uGt03arAiEA8V7IHRNnxM4ZEftvpph+SeoLtR5JlCPxR66RtNf3r0M=")]
        [InlineData("pIymao0C9p807BKL1d5PLlnnirXFSydp01YLxLoUL7Guyr7TsDnU2hAc3N8QZ9OsxDUHgRaV6H8b93LLA+F+zw==", "MEYCIQCkjKZqjQL2nzTsEovV3k8uWeeKtcVLJ2nTVgvEuhQvsQIhAK7KvtOwOdTaEBzc3xBn06zENQeBFpXofxv3cssD4X7P")]
        [InlineData("BzkDcOJC+DYSToVug3cQXT3WW8UdnivJiVgnQJyjbzhyaUC+az4xAmrFk/QGoDzuMkUYCf6NxtYWIpz6u795lA==", "MEQCIAc5A3DiQvg2Ek6FboN3EF091lvFHZ4ryYlYJ0Cco284AiByaUC+az4xAmrFk/QGoDzuMkUYCf6NxtYWIpz6u795lA==")]
        [InlineData("eta/GsI9cCcnZyknua8FW57XVYlGQrTsg0i+9FBqsDUf5xtCph763oawNuoyKG2d+Wqa2DlDLroEykfD3ZupJw==", "MEQCIHrWvxrCPXAnJ2cpJ7mvBVue11WJRkK07INIvvRQarA1AiAf5xtCph763oawNuoyKG2d+Wqa2DlDLroEykfD3ZupJw==")]
        [InlineData("lyD3Ieh3q903Wh2CqhELTHTxn9CCsxF8nq6o1zSaLlx/xl7UTCbEuNAjdHL2Rr3iSgBs9HP/F5QXcdJeVuCf5A==", "MEUCIQCXIPch6Her3TdaHYKqEQtMdPGf0IKzEXyerqjXNJouXAIgf8Ze1EwmxLjQI3Ry9ka94koAbPRz/xeUF3HSXlbgn+Q=")]
        [InlineData("aRCJ7UAwqjR5ZZGqRCw9Rk50EG1Skp9VJ2Rb/WuroP9Qt4QCAVFH4eLMuNo0NxMH25N0Hy2pixMGAReyEYFGvg==", "MEQCIGkQie1AMKo0eWWRqkQsPUZOdBBtUpKfVSdkW/1rq6D/AiBQt4QCAVFH4eLMuNo0NxMH25N0Hy2pixMGAReyEYFGvg==")]
        [InlineData("vEyhXS3U+h44WfJHtCgFmyM+fm3sXNeB0Bwyxs7LRTkC2XYBNWp85USa4JzRzLprpekEk13Fp7ZH5DTblFXHgw==", "MEUCIQC8TKFdLdT6HjhZ8ke0KAWbIz5+bexc14HQHDLGzstFOQIgAtl2ATVqfOVEmuCc0cy6a6XpBJNdxae2R+Q025RVx4M=")]
        public void Examples(string input, string result)
        {
            var inputBytes = Convert.FromBase64String(input);
            var resultBytes = Convert.FromBase64String(result);
            Assert.Equal(resultBytes, new X962PackagingFix().Format(inputBytes));
        }
    }
}
