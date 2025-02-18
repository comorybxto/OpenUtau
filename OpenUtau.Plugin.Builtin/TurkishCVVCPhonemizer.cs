using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using OpenUtau.Api;
using OpenUtau.Classic;
using OpenUtau.Core.Format.MusicXMLSchema;
using OpenUtau.Core.Ustx;
using Serilog;

namespace OpenUtau.Plugin.Builtin {
    [Phonemizer("Turkish CVVC Phonemizer", "TR CVVC", language: "TR")]
    public class TurkishCVVCPhonemizer : Phonemizer {
        static readonly string[] plainVowels = new string[] { "a", "e", "ae", "eu", "i", "o", "oe", "u", "ue", "L", "LY", "M", "N", "NG", "9a", "9e", "9ae", "9eu", "9i", "9o", "9oe", "9u", "9ue" };
        static readonly string[] nonVowels = "9,b,c,ch,d,f,g,h,j,k,l,m,n,p,r,rr,r',s,sh,t,v,w,y,z,by,dy,gy,hy,ky,ly,my,ny,py,ry,ty,-,?,q".Split(',');
        //static readonly string[] standaloneConsonants = new string[] { "ch", "f", "j", "r", "rr", "sh", "s", "t", "v", "y", "z", "k", "l", "m", "n", "p", "ky", "py", "ty" };

        static readonly string[] vowels = new string[] {
            "a=a,ba,ca,cha,da,fa,ga,gya,ha,ja,ka,kya,la,lya,ma,na,pa,ra,rra,r'a,sa,sha,ta,va,wa,ya,za,9a,qa,? a,bra,dra,gra,gla,hra,kra,kla,ksa,pra,pla,psa,tra,fra,fla,sla,ska,spa,sta,vla",
            "e=e,be,ce,che,de,fe,ge,he,je,ke,le,lye,me,ne,pe,re,rre,r'e,se,she,te,ve,we,ye,ze,9e,qe,? e,bre,dre,gre,gle,hre,kre,kle,kse,pre,ple,pse,tre,fre,fle,sle,ske,spe,ste,vle",
            "o=o,bo,co,cho,do,fo,go,ho,jo,ko,lo,lyo,mo,no,po,ro,rro,r'o,so,sho,to,vo,wo,yo,zo,9o,qo,? o,bro,dro,gro,glo,hro,kro,klo,kso,pro,plo,pso,tro,fro,flo,slo,sko,spo,sto,vlo",
            "u=u,bu,cu,chu,du,fu,gu,hu,ju,ku,lu,lyu,mu,nu,pu,ru,rru,r'u,su,shu,tu,vu,wu,yu,zu,9u,qu,? u,bru,dru,gru,glu,hru,kru,klu,ksu,pru,plu,psu,tru,fru,flu,slu,sku,spu,stu,vlu",
            "i=i,bi,ci,chi,di,fi,gi,gyi,hi,ji,ki,li,lyi,mi,ni,pi,ri,rri,r'i,si,shi,ti,vi,wi,yi,zi,9i,qi,? i,bri,dri,gri,gli,hri,kri,kli,ksi,pri,pli,psi,tri,fri,fli,sli,ski,spi,sti,vli",
            "ae=ae,bae,cae,chae,dae,fae,gae,hae,jae,kae,lae,lyae,mae,nae,pae,rae,rrae,r'ae,sae,shae,tae,vae,wae,yae,zae,9ae,qae,? ae,brae,drae,grae,glae,hrae,krae,klae,ksae,prae,plae,psae,trae,frae,flae,slae,skae,spae,stae,vlae",
            "eu=eu,beu,ceu,cheu,deu,feu,geu,heu,jeu,keu,leu,lyeu,meu,neu,peu,reu,rreu,r'eu,seu,sheu,teu,veu,weu,yeu,zeu,9eu,qeu,? eu,breu,dreu,greu,gleu,hreu,kreu,kleu,kseu,preu,pleu,pseu,treu,freu,fleu,sleu,skeu,speu,steu,vleu",
            "oe=oe,boe,coe,choe,doe,foe,goe,hoe,joe,koe,loe,lyoe,moe,noe,poe,roe,rroe,r'oe,soe,shoe,toe,voe,woe,yoe,zoe,9oe,qoe,? oe,broe,droe,groe,gloe,hroe,kroe,kloe,ksoe,proe,ploe,psoe,troe,froe,floe,sloe,skoe,spoe,stoe,vloe",
            "ue=ue,bue,cue,chue,due,fue,gue,hue,jue,kue,lue,lyue,mue,nue,pue,rue,rrue,r'ue,sue,shue,tue,vue,wue,yue,zue,9ue,que,? ue,brue,drue,grue,glue,hrue,krue,klue,ksue,prue,plue,psue,true,frue,flue,slue,skue,spue,stue,vlue",
            "N=N","M=M","NG=NG","L=L","LY=LY"
        };

        static readonly string[] consonants = new string[] {
            "b=b,ba,bae,beu,bo,bu,bra,brae,breu,bro,bru",
            "by=by,bi,be,boe,bue,bri,bre,broe,brue",
            "d=d,da,dae,deu,do,du,dra,drae,dreu,dro,dru",
            "dy=dy,di,de,doe,due,dri,dre,droe,drue",
            "g=g,ga,gae,geu,go,gu,gra,grae,greu,gro,gru,gla,glae,gleu,glo,glu",
            "gy=gy,gya,gi,ge,goe,gue,gri,gre,groe,grue,gli,gle,gloe,glue",
            "h=h,ha,hae,heu,ho,hu,hra,hrae,hreu,hro,hru",
            "hy=hy,hi,he,hoe,hue,hri,hre,hroe,hrue",
            "k=k,ka,kae,keu,ko,ku,kra,krae,kreu,kro,kru,kla,klae,kleu,klo,klu,ksa,ksae,kseu,kso,ksu",
            "ky=ky,kya,ki,ke,koe,kue,kri,kre,kroe,krue,kli,kle,kloe,klue,ksi,kse,ksoe,ksue",
            "l=l,la,lae,leu,lo,lu",
            "ly=ly,li,le,loe,lue,lya,lyu,lye,lyo",
            "m=m,ma,mae,meu,mo,mu",
            "my=my,mi,me,moe,mue",
            "n=n,na,nae,neu,no,nu",
            "ny=ny,ni,ne,noe,nue",
            "p=p,pa,pae,peu,po,pu,pra,prae,preu,pro,pru,pla,plae,pleu,plo,plu,psa,psae,pseu,pso,psu",
            "py=py,pi,pe,poe,pue,pri,pre,proe,prue,pli,ple,ploe,plue,psi,pse,psoe,psue",
            "r=r,ra,rae,reu,ro,ru",
            "ry=ry,ri,re,roe,rue",
            "t=t,ta,tae,teu,to,tu,tra,trae,treu,tro,tru",
            "ty=ty,ti,te,toe,tue,tri,tre,troe,true",
            "c=c,ca,ce,cae,ceu,ci,co,coe,cu,cue",
            "ch=ch,cha,che,chae,cheu,chi,cho,choe,chu,chue",
            "f=f,fa,fe,fae,feu,fi,fo,foe,fu,fue,fra,fre,frae,freu,fri,fro,froe,fru,frue,fla,fle,flae,fleu,fli,flo,floe,flu,flue",
            "j=j,ja,je,jae,jeu,ji,jo,joe,ju,jue",
            "rr=rr,rra,rre,rrae,rreu,rri,rro,rroe,rru,rrue",
            "r'=r',r'a,r'e,r'ae,r'eu,r'i,r'o,r'oe,r'u,r'ue",
            "s=s,sa,se,sae,seu,si,so,soe,su,sue,sla,sle,slae,sleu,sli,slo,sloe,slu,slue,ska,ske,skae,skeu,ski,sko,skoe,sku,skue,spa,spe,spae,speu,spi,spo,spoe,spu,spue,sta,ste,stae,steu,sti,sto,stoe,stu,stue",
            "sh=sh,sha,she,shae,sheu,shi,sho,shoe,shu,shue",
            "v=v,va,ve,vae,veu,vi,vo,voe,vu,vue,vla,vle,vlae,vleu,vli,vlo,vloe,vlu,vlue",
            "w=w,wa,we,wae,weu,wi,wo,woe,wu,wue",
            "y=y,ya,ye,yae,yeu,yi,yo,yoe,yu,yue",
            "z=z,za,ze,zae,zeu,zi,zo,zoe,zu,zue",
            "q=q,qa,qe,qae,qeu,qi,qo,qoe,qu,que",
            "?=?,? a,? e,? ae,? eu,? i,? o,? oe,? u,? ue"
        };

        // in case voicebank is missing certain symbols
        static readonly string[] substitution = new string[] {
            "by,br=b", "c,dr,dy,j=d", "fl,fr=f", "gl,gr,gy=g", "hr,hy=h", "kl,kr,ks,ky=k", "ly,L,LY=l", "my,M=m", "ny,ng,NG,N=n", "pl,pr,ps,py=p", "ry,rr,r'=r", "sk,sl,sp,st=s", "tr,ty,ch=t", "vl,w=v", "q=?"
        };

        static readonly Dictionary<string, string> vowelLookup;
        static readonly Dictionary<string, string> consonantLookup;
        //static readonly Dictionary<string, string> standaloneConsonantLookup;
        static readonly Dictionary<string, string> substituteLookup;

        static TurkishCVVCPhonemizer() {
            vowelLookup = vowels.ToList()
                .SelectMany(line => {
                    var parts = line.Split('=');
                    return parts[1].Split(',').Select(cv => (cv, parts[0]));
                })
                .ToDictionary(t => t.Item1, t => t.Item2);
            consonantLookup = consonants.ToList()
                .SelectMany(line => {
                    var parts = line.Split('=');
                    return parts[1].Split(',').Select(cv => (cv, parts[0]));
                })
                .ToDictionary(t => t.Item1, t => t.Item2);
            /*
             * standaloneConsonantLookup = standaloneConsonants.ToList()
                .SelectMany(line => {
                    var parts = line.Split('=');
                    return parts[1].Split(',').Select(cv => (cv, parts[0]));
                })
                .ToDictionary(t => t.Item1, t => t.Item2);
            */
            substituteLookup = substitution.ToList()
                .SelectMany(line => {
                    var parts = line.Split('=');
                    return parts[0].Split(',').Select(orig => (orig, parts[1]));
                })
                .ToDictionary(t => t.Item1, t => t.Item2);
        }

        // Store singer in field, will try reading presamp.ini later
        private USinger singer;
        public override void SetSinger(USinger singer) => this.singer = singer;

        // make it quicker to check multiple oto occurrences at once rather than spamming if else if
        private bool checkOtoUntilHit(string[] input, Note note, out UOto oto) {
            oto = default;
            var attr = note.phonemeAttributes?.FirstOrDefault(attr => attr.index == 0) ?? default;

            var otos = new List<UOto>();
            foreach (string test in input) {
                if (singer.TryGetMappedOto(test + attr.alternate, note.tone + attr.toneShift, attr.voiceColor, out var otoAlt)) {
                    otos.Add(otoAlt);
                } else if (singer.TryGetMappedOto(test, note.tone + attr.toneShift, attr.voiceColor, out var otoCandidacy)) {
                    otos.Add(otoCandidacy);
                }
            }

            string color = attr.voiceColor ?? "";
            if (otos.Count > 0) {
                if (otos.Any(oto => (oto.Color ?? string.Empty) == color)) {
                    oto = otos.Find(oto => (oto.Color ?? string.Empty) == color);
                    return true;
                } else {
                    oto = otos.First();
                    return true;
                }
            }
            return false;
        }

        // checking VCs
        // when VC does not exist, it will not be inserted
        private bool checkOtoUntilHitVc(string[] input, Note note, out UOto oto) {
            oto = default;
            var attr = note.phonemeAttributes?.FirstOrDefault(attr => attr.index == 1) ?? default;

            var otos = new List<UOto>();
            foreach (string test in input) {
                if (singer.TryGetMappedOto(test + attr.alternate, note.tone + attr.toneShift, attr.voiceColor, out var otoAlt)) {
                    otos.Add(otoAlt);
                } else if (singer.TryGetMappedOto(test, note.tone + attr.toneShift, attr.voiceColor, out var otoCandidacy)) {
                    otos.Add(otoCandidacy);
                }
            }

            string color = attr.voiceColor ?? "";
            if (otos.Count > 0) {
                if (otos.Any(oto => (oto.Color ?? string.Empty) == color)) {
                    oto = otos.Find(oto => (oto.Color ?? string.Empty) == color);
                    return true;
                } else {
                    return false;
                }
            }
            return false;
        }


        // can probably be cleaned up more but i have work in the morning. have fun.
        public override Result Process(Note[] notes, Note? prev, Note? next, Note? prevNeighbour, Note? nextNeighbour, Note[] prevNeighbours) {
            var note = notes[0];
            var currentLyric = note.lyric.Normalize();
            if (!string.IsNullOrEmpty(note.phoneticHint)) {
                currentLyric = note.phoneticHint.Normalize();
            }
            var originalCurrentLyric = currentLyric;
            var cfLyric = $"* {currentLyric}";
            var attr0 = note.phonemeAttributes?.FirstOrDefault(attr => attr.index == 0) ?? default;
            var attr1 = note.phonemeAttributes?.FirstOrDefault(attr => attr.index == 1) ?? default;

            if (!string.IsNullOrEmpty(note.phoneticHint)) {
                string[] tests = new string[] { currentLyric };
                // Not convert VCV
                if (checkOtoUntilHit(tests, note, out var oto)) {
                    currentLyric = oto.Alias;
                }
            } else if (prevNeighbour == null) {
                // Use "- V" or "- CV" if present in voicebank
                var initial = $"- {currentLyric}";
                string[] tests = new string[] { initial, currentLyric };
                // try [- XX] before trying plain lyric
                if (checkOtoUntilHit(tests, note, out var oto)) {
                    currentLyric = oto.Alias;
                }
            } else if (plainVowels.Contains(currentLyric) || nonVowels.Contains(currentLyric)) {
                var prevLyric = prevNeighbour.Value.lyric.Normalize();
                if (!string.IsNullOrEmpty(prevNeighbour.Value.phoneticHint)) {
                    prevLyric = prevNeighbour.Value.phoneticHint.Normalize();
                }
                // Current note is VV
                if (vowelLookup.TryGetValue(prevLyric.LastOrDefault().ToString() ?? string.Empty, out var vow)) {
                    var vowLyric = $"{vow} {currentLyric}";
                    // try vowlyric before cflyric, if both fail try currentlyric
                    string[] tests = new string[] { vowLyric, cfLyric, currentLyric };
                    if (checkOtoUntilHit(tests, note, out var oto)) {
                        currentLyric = oto.Alias;
                    }
                }
            } else {
                string[] tests = new string[] { cfLyric, currentLyric };
                if (checkOtoUntilHit(tests, note, out var oto)) {
                    currentLyric = oto.Alias;
                }
            }
            
            var tempLyric = currentLyric;

            if (nextNeighbour == null && consonantLookup.ContainsValue(originalCurrentLyric.Substring(0, Math.Min(3, originalCurrentLyric.Length)))) {
                var vowel = "";
                var consonant = "";
                var prevLyric = prevNeighbour.Value.lyric.Normalize();

                if (nonVowels.Contains(currentLyric)) {
                    if (consonantLookup.TryGetValue(prevLyric.LastOrDefault().ToString().ToLower() ?? string.Empty, out var con)) {
                        consonant = con;
                        var conLyric = $"{consonant} {currentLyric}";
                        string[] tests = new string[] { conLyric, currentLyric };
                        if (checkOtoUntilHit(tests, note, out var oto)) {
                            currentLyric = $"{consonant}{originalCurrentLyric} -{oto.Suffix}";
                        }
                    }
                } else {
                    if (vowelLookup.TryGetValue(prevLyric.LastOrDefault().ToString() ?? string.Empty, out var vow)) {
                        vowel = vow;
                        var vowLyric = $"{vowel} {currentLyric}";
                        string[] tests = new string[] { vowLyric, currentLyric };
                        if (checkOtoUntilHit(tests, note, out var oto)) {
                            currentLyric = $"{vowel} {originalCurrentLyric}-{oto.Suffix}";
                        }
                    }
                }

                return new Result {
                    phonemes = new Phoneme[] {
                            new Phoneme() {
                                phoneme = currentLyric,
                            }
                    },
                };
            }

            if (nextNeighbour != null && string.IsNullOrEmpty(nextNeighbour.Value.phoneticHint)) {
                var nextLyric = nextNeighbour.Value.lyric.Normalize();
                var prevLyric = prevNeighbour.Value.lyric.Normalize();
                var vowel = "";
                var consonant = "";

                if (consonantLookup.ContainsKey(originalCurrentLyric.Substring(0, originalCurrentLyric.Length))) { // key: ka | value: k

                    //DOESN'T WORK AS INTENDED
                    //case where nextLyric == C (this one works :thumbsup:)
                    if (nonVowels.Contains(currentLyric)) {
                        if (consonantLookup.TryGetValue(prevLyric.LastOrDefault().ToString().ToLower() ?? string.Empty, out var con)) {
                            consonant = con;
                            var conLyric = $"{consonant} {currentLyric}";
                            string[] tests = new string[] { conLyric, currentLyric };
                            if (checkOtoUntilHit(tests, note, out var oto)) {
                                currentLyric = $"{consonant} {originalCurrentLyric}{oto.Suffix}";
                            }

                            return new Result {
                                phonemes = new Phoneme[] {
                                    new Phoneme() {
                                        phoneme = currentLyric,
                                    }
                                },
                            };
                        }
                    } else if (consonantLookup.ContainsValue(currentLyric)) { //PROBLEM BLOCK!!!!!!!!!!!!!!!!!!!!!!!!!! H8
                        if (consonantLookup.TryGetValue(prevLyric.LastOrDefault().ToString().ToLower() ?? string.Empty, out var con)) {
                            consonant = con;
                            var conLyric = $"{consonant} {currentLyric}";
                            string[] tests = new string[] { conLyric, currentLyric };
                            if (checkOtoUntilHit(tests, note, out var oto)) {
                                currentLyric = $".CC_CV.{consonant} {originalCurrentLyric}{oto.Suffix}";
                            }

                            return new Result {
                                phonemes = new Phoneme[] {
                                    new Phoneme() {
                                        phoneme = currentLyric,
                                    }
                                },
                            };
                        }
                    }
                }
            } //boka sardı geri al. aşağıdakilerin çalışmasını engelliyor

            currentLyric = tempLyric;

            if (nextNeighbour != null && string.IsNullOrEmpty(nextNeighbour.Value.phoneticHint)) {
                var nextLyric = nextNeighbour.Value.lyric.Normalize();


                // Check if next note is a vowel and does not require VC
                if (nextLyric.Length == 1 && plainVowels.Contains(nextLyric)) {
                    return new Result {
                        phonemes = new Phoneme[] {
                            new Phoneme() {
                                phoneme = currentLyric,
                            }
                        },
                    };
                }


                // Insert VC before next neighbor
                // Get vowel from current note
                var vowel = "";
                if (vowelLookup.TryGetValue(originalCurrentLyric.LastOrDefault().ToString() ?? string.Empty, out var vow)) {
                    vowel = vow;
                }

                var consonant = "";
                if (nextLyric.Length >= 3 && consonantLookup.TryGetValue(nextLyric.Substring(0, 3), out var con)) {
                    consonant = con; // Handle cases like "kya" and "lya"
                } else if (nextLyric.Length >= 2 && consonantLookup.TryGetValue(nextLyric.Substring(0, 2), out con)) {
                    consonant = con; // Handle cases like "ka" and "la"
                }

                if (consonant == "") {
                    return new Result {
                        phonemes = new Phoneme[] {
                            new Phoneme() {
                                phoneme = currentLyric,
                            }
                        },
                    };
                }


                var vcPhoneme = $"{vowel} {consonant}";
                var vcPhonemes = new string[] { vcPhoneme, "" };
                // find potential substitute symbol
                if (substituteLookup.TryGetValue(consonant ?? string.Empty, out con)) {
                    vcPhonemes[1] = $"{vowel} {con}";
                }
                //if (singer.TryGetMappedOto(vcPhoneme, note.tone + attr0.toneShift, attr0.voiceColor, out var oto1)) {
                if (checkOtoUntilHitVc(vcPhonemes, note, out var oto1)) {
                    vcPhoneme = oto1.Alias;
                } else {
                    return new Result {
                        phonemes = new Phoneme[] {
                            new Phoneme() {
                                phoneme = currentLyric,
                            }
                        },
                    };
                }

                int totalDuration = notes.Sum(n => n.duration);
                int vcLength = 120;
                var nextAttr = nextNeighbour.Value.phonemeAttributes?.FirstOrDefault(attr => attr.index == 0) ?? default;
                if (singer.TryGetMappedOto(nextLyric, nextNeighbour.Value.tone + nextAttr.toneShift, nextAttr.voiceColor, out var oto)) {
                    // If overlap is a negative value, vcLength is longer than Preutter
                    if (oto.Overlap < 0) {
                        vcLength = MsToTick(oto.Preutter - oto.Overlap);
                    } else {
                        vcLength = MsToTick(oto.Preutter);
                    }
                }
                // vcLength depends on the Vel of the next note
                vcLength = Convert.ToInt32(Math.Min(totalDuration / 2, vcLength * (nextAttr.consonantStretchRatio ?? 1)));

                return new Result {
                    phonemes = new Phoneme[] {
                        new Phoneme() {
                            phoneme = currentLyric,
                        },
                        new Phoneme() {
                            phoneme = vcPhoneme,
                            position = totalDuration - vcLength,
                        }
                    },
                };
            }

            // No next neighbor
            return new Result {
                phonemes = new Phoneme[] {
                    new Phoneme {
                        phoneme = currentLyric,
                    }
                },
            };
        }
    }

}
