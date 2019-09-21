using UnityEngine;

namespace mardevmil.Core
{
    public static class PhysicMath
    {
        /// <summary>
        /// Calculation velocity with given target and angle
        /// </summary>
        /// <param name="target"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static Vector3 CalculateVelocity(Transform velocityObject, Transform target, float angle)
        {
            var direction = target.position - velocityObject.position;  // get target direction            
            var heightDiff = direction.y;  // get height difference
            direction.y = 0;  // retain only the horizontal direction
            var distance = direction.magnitude;  // get horizontal distance

            var angleRadians = angle * Mathf.Deg2Rad;  // convert angle to radians
            var tan = Mathf.Tan(angleRadians);
            
            direction.y = distance * Mathf.Tan(angleRadians);  // set direction to the elevation angle
            distance += heightDiff / Mathf.Tan(angleRadians);  // correct for small height differences
            
            // calculate the velocity magnitude
            var velocity = Mathf.Sqrt(distance * Physics.gravity.magnitude / Mathf.Sin(2 * angleRadians));            
            return velocity * direction.normalized;


        }

        /// <summary>
        /// Calculation projectile velocity with given target and angle
        /// </summary>
        /// <param name="target"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static Vector3 CalculateProjectileVelocity(Transform projectile, Transform target, float angle)
        {
            //think of it as top - down view of vectors:
            //we don't care about the y-component(height) of the initial and target position.
            Vector3 projectileXZPos = new Vector3(projectile.position.x, 0.0f, projectile.position.z);
            Vector3 targetXZPos = new Vector3(target.position.x, 0.0f, target.position.z);

            // rotate the object to face the target
            projectile.LookAt(targetXZPos);

            // shorthands for the formula
            float R = Vector3.Distance(projectileXZPos, targetXZPos);
            float G = Physics.gravity.y;
            float tanAlpha = Mathf.Tan(angle * Mathf.Deg2Rad);
            float H = target.position.y - projectile.position.y;

            // calculate the local space components of the velocity 
            // required to land the projectile on the target object 
            float Vz = Mathf.Sqrt(G * R * R / (2.0f * (H - R * tanAlpha)));
            float Vy = tanAlpha * Vz;

            // create the velocity vector in local space and get it in global space
            Vector3 localVelocity = new Vector3(0f, Vy, Vz);
            Vector3 globalVelocity = projectile.TransformDirection(localVelocity);

            return globalVelocity;

        }

    }

}