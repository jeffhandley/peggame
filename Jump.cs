using System;

namespace peggame
{
    struct Jump
    {
        public char From {get; set;}
        public char To {get; set;}
        public char Over {get; set;}

        public override bool Equals(object obj) {
            if (!(obj is Jump)) {
                return false;
            }

            Jump jump = (Jump)obj;

            if (jump.From == this.From && jump.Over == this.Over) {
                return true;
            }

            return false;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}
